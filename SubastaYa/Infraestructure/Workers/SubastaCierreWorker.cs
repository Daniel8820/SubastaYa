using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Domain.Entities;// Asegúrense de usar sus namespaces exactos
using Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            // 1. Buscamos subastas activas cuyo tiempo ya se agotó
            var subastasVencidas = await context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.Estado == "ACTIVA" && s.FechaFin <= DateTime.Now)
                .ToListAsync();

            foreach (var subasta in subastasVencidas)
            {
                var pujaGanadora = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();

                if (pujaGanadora != null)
                {
                    // 1. Acreditamos al Vendedor
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
                            Fecha = DateTime.Now,
                            SubastaId = subasta.Id
                        });
                    }

                    // 2. NUEVO: Debitamos el saldo retenido al Comprador ganador de forma definitiva
                    var billeteraComprador = await context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == pujaGanadora.CompradorId);
                    if (billeteraComprador != null)
                    {
                        billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                        billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;

                        context.TransaccionesLedger.Add(new TransaccionLedger
                        {
                            BilleteraId = billeteraComprador.Id,
                            Tipo = "PAGO_SUBASTA", // O "DEBITO"
                            Monto = pujaGanadora.Monto,
                            Fecha = DateTime.Now,
                            SubastaId = subasta.Id
                        });
                    }

                    subasta.Estado = "FINALIZADA";
                }
                else
                {
                    // 3. No hubo ofertas
                    subasta.Estado = "DESIERTA";
                }
            }

            if (subastasVencidas.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}