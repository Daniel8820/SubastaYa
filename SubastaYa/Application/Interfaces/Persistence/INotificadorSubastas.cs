namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface INotificadorSubastas
    {
        // Método para aviso global de la suba de precio
        Task NotificarNuevaPujaAsync(int subastaId, decimal nuevoMonto, string compradorNombre, int compradorId);
    }
}