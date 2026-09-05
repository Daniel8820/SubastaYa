using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISubastaRepository
    {
        Task<Subasta?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Subasta subasta);
        void Actualizar(Subasta subasta);
        Task<List<Domain.Entities.Subasta>> ObtenerPorVendedorIdAsync(int vendedorId);
        Task<List<Domain.Entities.Subasta>> ObtenerPorCompradorIdAsync(int compradorId);
    }
}