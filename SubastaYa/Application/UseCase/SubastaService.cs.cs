using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Services
{
    public class SubastaService : ISubastaService
    {
        private readonly SubastaYaDbContext _context;

        public SubastaService(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarPujaAsync(RegistroPujaRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var subasta = await _context.Subastas
                    .Include(s => s.Pujas)
                    .FirstOrDefaultAsync(s => s.Id == request.SubastaId);

                if (subasta == null)
                {
                    throw new Exception("La subasta no existe.");
                }

                if (subasta.Estado != "ACTIVA" || subasta.FechaFin <= DateTime.UtcNow)
                {
                    throw new Exception("La subasta ya ha finalizado o no se encuentra activa.");
                }

                // TODO 2: Validar el monto de la oferta y el incremento mínimo
                var ofertaMasAlta = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase;
                var montoMinimoRequerido = subasta.Pujas.Any() ? ofertaMasAlta + subasta.IncrementoMinimo : subasta.PrecioBase;

                if (request.Monto < montoMinimoRequerido)
                {
                    // NUEVO: Auditoría de rechazo por monto
                    _context.AuditoriaLogs.Add(new AuditoriaLog
                    {
                        Entidad = "SUBASTA",
                        EntidadId = request.SubastaId,
                        Accion = "PUJA_RECHAZADA",
                        UsuarioId = request.CompradorId,
                        DetalleJson = $"{{ \"motivo\": \"Monto insuficiente. Requerido: {montoMinimoRequerido}, Intentado: {request.Monto}\" }}",
                        Fecha = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); // Commiteamos el log antes de lanzar el error

                    throw new Exception($"El monto de la puja es inválido. Debe ser de al menos ${montoMinimoRequerido}.");
                }

                // TODO 3: Traer la billetera del Comprador y validar saldo
                var billeteraComprador = await _context.Billeteras
                    .FirstOrDefaultAsync(b => b.UsuarioId == request.CompradorId);

                if (billeteraComprador == null)
                {
                    throw new Exception("El comprador no tiene una billetera asociada.");
                }

                if (billeteraComprador.SaldoDisponible < request.Monto)
                {
                    // NUEVO: Auditoría de rechazo por saldo
                    _context.AuditoriaLogs.Add(new AuditoriaLog
                    {
                        Entidad = "BILLETERA",
                        EntidadId = billeteraComprador.Id,
                        Accion = "PUJA_RECHAZADA",
                        UsuarioId = request.CompradorId,
                        DetalleJson = $"{{ \"motivo\": \"Saldo insuficiente. Disponible: {billeteraComprador.SaldoDisponible}, Intentado: {request.Monto}\" }}",
                        Fecha = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    throw new Exception("Saldo insuficiente para realizar esta puja.");
                }

                // TODO 4: LÓGICA DE ESCROW (Garantía)
                var pujaAnterior = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
                if (pujaAnterior != null)
                {
                    var billeteraAnterior = await _context.Billeteras
                        .FirstOrDefaultAsync(b => b.UsuarioId == pujaAnterior.CompradorId);

                    if (billeteraAnterior != null)
                    {
                        billeteraAnterior.SaldoRetenido -= pujaAnterior.Monto;
                        billeteraAnterior.SaldoDisponible += pujaAnterior.Monto;

                        _context.TransaccionesLedger.Add(new TransaccionLedger
                        {
                            BilleteraId = billeteraAnterior.Id,
                            Tipo = "LIBERACION",
                            Monto = pujaAnterior.Monto,
                            Fecha = DateTime.UtcNow,
                            SubastaId = subasta.Id
                        });
                    }
                }

                billeteraComprador.SaldoDisponible -= request.Monto;
                billeteraComprador.SaldoRetenido += request.Monto;

                _context.TransaccionesLedger.Add(new TransaccionLedger
                {
                    BilleteraId = billeteraComprador.Id,
                    Tipo = "RETENCION",
                    Monto = request.Monto,
                    Fecha = DateTime.UtcNow,
                    SubastaId = subasta.Id
                });

                // TODO 5: REGLA ANTI-SNIPING CON AUDITORÍA
                var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;

                if (tiempoRestante.TotalSeconds > 0 && tiempoRestante.TotalSeconds <= 60)
                {
                    subasta.FechaFin = subasta.FechaFin.AddMinutes(2);

                    // NUEVO: Auditoría de extensión
                    _context.AuditoriaLogs.Add(new AuditoriaLog
                    {
                        Entidad = "SUBASTA",
                        EntidadId = subasta.Id,
                        Accion = "EXTENSION_ANTI_SNIPING",
                        UsuarioId = request.CompradorId,
                        DetalleJson = $"{{ \"mensaje\": \"Extensión automática por oferta en último minuto\", \"nuevaFechaFin\": \"{subasta.FechaFin}\" }}",
                        Fecha = DateTime.UtcNow
                    });
                }

                // TODO 7: Crear y registrar la nueva Puja
                var nuevaPuja = new Puja
                {
                    SubastaId = subasta.Id,
                    CompradorId = request.CompradorId,
                    Monto = request.Monto,
                    FechaPuja = DateTime.UtcNow
                };

                _context.Pujas.Add(nuevaPuja);

                subasta.Version++;

                try
                {
                    // 1. Intentamos guardar la puja, los saldos y la nueva versión de la subasta
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true; // O el objeto que devuelva tu método en caso de éxito
                }
                catch (DbUpdateConcurrencyException)
                {
                    // 2. ¡Choque detectado! Dos usuarios pujaron a la vez. Abortamos la transacción original.
                    await transaction.RollbackAsync();

                    // 3. ¡CRÍTICO! Limpiamos el ChangeTracker para que EF Core "olvide" 
                    // la puja fallida y los cálculos de saldo erróneos.
                    _context.ChangeTracker.Clear();

                    // 4. Armamos el registro para la auditoría (Requisito cumplido)
                    var logConcurrencia = new AuditoriaLog
                    {
                        Entidad = "SUBASTA",
                        EntidadId = subasta.Id,
                        Accion = "PUJA_RECHAZADA_CONCURRENCIA",
                        UsuarioId = request.CompradorId,
                        DetalleJson = $"{{ \"mensaje\": \"Rechazo por concurrencia. Otro usuario pujó al mismo tiempo.\", \"montoIntentado\": {request.Monto} }}",
                        Fecha = DateTime.UtcNow
                    };

                    _context.AuditoriaLogs.Add(logConcurrencia);

                    // 5. Guardamos el log tranquilamente en una operación limpia
                    await _context.SaveChangesAsync();

                    // 6. Retornamos falso (o podés lanzar una excepción de negocio si tu controller lo maneja así)
                    return false;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new Exception("Conflicto de concurrencia: la subasta fue modificada por otro usuario.");
            }
            catch (Exception ex)
            {
                // Si la excepción no fue lanzada por nuestras validaciones (que ya commitearon el log), hacemos rollback general.
                // Como los throw de arriba tiran excepciones genéricas ("Exception"), acá podríamos loguear un error inesperado.
                if (transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }
    }
}
