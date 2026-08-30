using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
            // Iniciamos la transacción atómica (ACID). Si algo falla, se hace rollback automático.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // TODO 1: Traer la subasta de la BD y verificar estado
                // Usamos Include para traer las Pujas actuales y evaluar la oferta líder
                var subasta = await _context.Subastas
                    .Include(s => s.Pujas)
                    .FirstOrDefaultAsync(s => s.Id == request.SubastaId);

                if (subasta == null)
                {
                    throw new Exception("La subasta no existe.");
                }

                // Validamos que el estado sea ACTIVA y que no se haya vencido el tiempo
                if (subasta.Estado != "ACTIVA" || subasta.FechaFin <= DateTime.Now)
                {
                    throw new Exception("La subasta ya ha finalizado o no se encuentra activa.");
                }

                // TODO 2: Validar el monto de la oferta y el incremento mínimo
                var ofertaMasAlta = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase;
                var montoMinimoRequerido = subasta.Pujas.Any() ? ofertaMasAlta + subasta.IncrementoMinimo : subasta.PrecioBase;

                if (request.Monto < montoMinimoRequerido)
                {
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
                    throw new Exception("Saldo insuficiente para realizar esta puja.");
                }

                // TODO 4: LÓGICA DE ESCROW (Garantía)
                // 4.1. Devolver el dinero al comprador anterior (si existe)
                var pujaAnterior = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
                if (pujaAnterior != null)
                {
                    var billeteraAnterior = await _context.Billeteras
                        .FirstOrDefaultAsync(b => b.UsuarioId == pujaAnterior.CompradorId);

                    if (billeteraAnterior != null)
                    {
                        // Liberamos los fondos del perdedor
                        billeteraAnterior.SaldoRetenido -= pujaAnterior.Monto;
                        billeteraAnterior.SaldoDisponible += pujaAnterior.Monto;

                        // TODO 6: Registro en el Ledger para la devolución
                        _context.TransaccionesLedger.Add(new TransaccionLedger
                        {
                            BilleteraId = billeteraAnterior.Id,
                            Tipo = "LIBERACION",
                            Monto = pujaAnterior.Monto,
                            Fecha = DateTime.Now,
                            SubastaId = subasta.Id
                        });
                    }
                }

                // 4.2. Retener los fondos del nuevo comprador
                billeteraComprador.SaldoDisponible -= request.Monto;
                billeteraComprador.SaldoRetenido += request.Monto;

                // Registro en el Ledger para la nueva retención
                _context.TransaccionesLedger.Add(new TransaccionLedger
                {
                    BilleteraId = billeteraComprador.Id,
                    Tipo = "RETENCION",
                    Monto = request.Monto,
                    Fecha = DateTime.Now,
                    SubastaId = subasta.Id
                });

                // TODO 5: REGLA ANTI-SNIPING
                var tiempoRestante = subasta.FechaFin - DateTime.Now;
                if (tiempoRestante.TotalMinutes < 1)
                {
                    subasta.FechaFin = subasta.FechaFin.AddMinutes(2);

                    // Opcional: Podrían agregar un registro en AuditoriaLogs acá para dejar rastro de la extensión
                }

                // TODO 7: Crear y registrar la nueva Puja
                var nuevaPuja = new Puja
                {
                    SubastaId = subasta.Id,
                    CompradorId = request.CompradorId,
                    Monto = request.Monto,
                    FechaPuja = DateTime.Now
                };

                _context.Pujas.Add(nuevaPuja);

                // Guardamos los cambios. Aquí EF Core validará el Optimistic Locking (el campo Version).
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si EF Core detecta que alguien más modificó la subasta o la billetera en el mismo instante
                await transaction.RollbackAsync();
                throw new Exception("Conflicto de concurrencia: la subasta fue modificada por otro usuario.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Aquí podrían loguear el error o usar el AuditoriaLog
                throw;
            }
        }
    }
}
