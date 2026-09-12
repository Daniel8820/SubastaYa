using SubastaYa.Application.Interfaces.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Infrastructure.SignalR
{
    public class NotificadorSubastas : INotificadorSubastas
    {
        private readonly IHubContext<SubastaHub> _hubContext;

        public NotificadorSubastas(IHubContext<SubastaHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Le sumamos el parámetro "int compradorId" al final
        public async Task NotificarNuevaPujaAsync(int subastaId, decimal nuevoMonto, string compradorNombre, int compradorId)
        {
            string nombreSala = $"subasta_{subastaId}";

            await _hubContext.Clients.Group(nombreSala).SendAsync("RecibirNuevaPuja", new
            {
                Monto = nuevoMonto,
                Comprador = compradorNombre,
                CompradorId = compradorId,
                Fecha = DateTime.UtcNow
            });
        }
    }
}