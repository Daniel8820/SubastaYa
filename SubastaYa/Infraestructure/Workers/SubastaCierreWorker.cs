using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SubastaYa.Presentacion.Workers
{
    public class SubastaCierreWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SubastaCierreWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // El worker correrá en un bucle infinito mientras la API esté levantada
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcesarSubastasVencidasAsync();

                // Pausa de 1 minuto antes de volver a revisar la base de datos
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcesarSubastasVencidasAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SubastaYaDbContext>();

            // Activar subastas programadas
            var subastasParaActivar = await context.Subastas
                .Where(s => s.Estado == "PROGRAMADA" && s.FechaInicio <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var subasta in subastasParaActivar)
            {
                subasta.Estado = "ACTIVA";

                // Registramos en auditoría la activación automática
                context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "ACTIVACION_AUTOMATICA",
                    UsuarioId = null, // Realizado por el sistemas
                    DetalleJson = "{ \"estadoAnterior\": \"PROGRAMADA\", \"nuevoEstado\": \"ACTIVA\" }",
                    Fecha = DateTime.UtcNow
                });
            }
            // Finalizar subastas vencidas
            var subastasVencidas = await context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.Estado == "ACTIVA" && s.FechaFin <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var subasta in subastasVencidas)
            {
                var pujaGanadora = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
                string estadoAnterior = subasta.Estado;
                string nuevoEstado;

                if (pujaGanadora != null)
                {
                    // Acreditamos al Vendedor
                    var billeteraVendedor = await context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == subasta.VendedorId);
                    if (billeteraVendedor != null)
                    {
                        billeteraVendedor.SaldoTotal += pujaGanadora.Monto;
                        billeteraVendedor.SaldoDisponible += pujaGanadora.Monto;

                        context.TransaccionesLedger.Add(new TransaccionLedger
                        {
                            BilleteraId = billeteraVendedor.Id,
                            Tipo = "ACREDITACION_VENTA",
                            Monto = pujaGanadora.Monto,
                            Fecha = DateTime.UtcNow,
                            SubastaId = subasta.Id
                        });
                    }

                    // Debitamos el saldo retenido al Comprador ganador
                    var billeteraComprador = await context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == pujaGanadora.CompradorId);
                    if (billeteraComprador != null)
                    {
                        billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                        billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;

                        context.TransaccionesLedger.Add(new TransaccionLedger
                        {
                            BilleteraId = billeteraComprador.Id,
                            Tipo = "PAGO_SUBASTA",
                            Monto = pujaGanadora.Monto,
                            Fecha = DateTime.UtcNow,
                            SubastaId = subasta.Id
                        });
                    }

                    nuevoEstado = "FINALIZADA";
                }
                else
                {
                    nuevoEstado = "DESIERTA";
                }

                subasta.Estado = nuevoEstado;

                context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "CAMBIO_ESTADO",
                    UsuarioId = null,
                    DetalleJson = $"{{ \"estadoAnterior\": \"{estadoAnterior}\", \"nuevoEstado\": \"{nuevoEstado}\" }}",
                    Fecha = DateTime.UtcNow
                });
            }

            // Guardar cambios de ambos procesos
            if (subastasVencidas.Any() || subastasParaActivar.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}