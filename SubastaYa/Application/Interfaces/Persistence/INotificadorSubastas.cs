namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface INotificadorSubastas
    {
        // Método para avisar a todos en la sala que el precio subió
        Task NotificarNuevaPujaAsync(int subastaId, decimal nuevoMonto, string compradorNombre);
    }
}