using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISubastaRepository
    {
        Task<Subasta?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Subasta subasta);
        void Actualizar(Subasta subasta);
    }
}