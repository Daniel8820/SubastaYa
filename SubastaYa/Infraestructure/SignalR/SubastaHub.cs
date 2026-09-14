using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Infrastructure.SignalR
{
    public class SubastaHub : Hub
    {
        // React llama a este método al entrar a la vista de la subasta
        public async Task UnirseASala(int subastaId)
        {
            // Agrupamos a los usuarios por el ID de la subasta
            string nombreSala = $"subasta_{subastaId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, nombreSala);
        }

        // React llama a este método si el usuario sale de la vista
        public async Task SalirDeSala(int subastaId)
        {
            string nombreSala = $"subasta_{subastaId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nombreSala);
        }
    }
}