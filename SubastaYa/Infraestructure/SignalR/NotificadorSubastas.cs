using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR
{
    public class NotificadorSubastas : INotificadorSubastas
    {
        private readonly IHubContext<SubastaHub> _hubContext;

        // Inyectamos el contexto del Hub de SignalR
        public NotificadorSubastas(IHubContext<SubastaHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotificarNuevaPujaAsync(int subastaId, decimal nuevoMonto, string compradorNombre)
        {
            string nombreSala = $"subasta_{subastaId}";

            // Disparamos un evento llamado "RecibirNuevaPuja" SOLO a los que estén mirando esta subasta
            await _hubContext.Clients.Group(nombreSala).SendAsync("RecibirNuevaPuja", new
            {
                Monto = nuevoMonto,
                Comprador = compradorNombre,
                Fecha = DateTime.UtcNow
            });
        }
    }
}