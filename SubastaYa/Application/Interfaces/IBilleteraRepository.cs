using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBilleteraRepository
    {
        Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId);
        void Actualizar(Billetera billetera);
    }
}