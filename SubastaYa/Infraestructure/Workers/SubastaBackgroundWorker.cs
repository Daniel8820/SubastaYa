using SubastaYa.Application.UseCases.Subastas.ActivarSubastas;
using SubastaYa.Application.UseCases.Subastas.FinalizarSubastas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SubastaYa.Infrastructure.Workers
{
    public class SubastaBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubastaBackgroundWorker> _logger;

        public SubastaBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<SubastaBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker de Subastas iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Como el Worker es Singleton, necesitamos crear un Scope para traer los Handlers que son Scoped
                    using var scope = _scopeFactory.CreateScope();

                    var activarHandler = scope.ServiceProvider.GetRequiredService<ActivarSubastasCommandHandler>();
                    var finalizarHandler = scope.ServiceProvider.GetRequiredService<FinalizarSubastasCommandHandler>();

                    // 1. Ejecutamos el caso de uso de activación
                    await activarHandler.HandleAsync(new ActivarSubastasCommand());

                    // 2. Ejecutamos el caso de uso de cierre
                    await finalizarHandler.HandleAsync(new FinalizarSubastasCommand());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error en el ciclo del Worker de Subastas.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}