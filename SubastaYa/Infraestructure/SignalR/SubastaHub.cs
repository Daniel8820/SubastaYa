using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Infrastructure.SignalR
{
    public class SubastaHub : Hub
    {
        // React llamará a este método al entrar a la vista de la subasta
        public async Task UnirseASala(int subastaId)
        {
            // Agrupamos a los usuarios por el ID de la subasta
            string nombreSala = $"subasta_{subastaId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, nombreSala);
        }

        // React llamará a este método si el usuario sale de la vista
        public async Task SalirDeSala(int subastaId)
        {
            string nombreSala = $"subasta_{subastaId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nombreSala);
        }
    }
}