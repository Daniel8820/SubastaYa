using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface ISubastaRepository
    {
        Task<Subasta?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Subasta subasta);
        void Actualizar(Subasta subasta);
        Task<List<Subasta>> ObtenerPorVendedorIdAsync(int vendedorId);
        Task<List<Subasta>> ObtenerPorCompradorIdAsync(int compradorId);
        Task<Subasta?> ObtenerDetallePorIdAsync(int id);
        Task<(List<Subasta> Items, int TotalItems)> ObtenerCatalogoPaginadoAsync(string estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string orden, int pagina, int tamañoPagina);
        void AgregarAuditoria(AuditoriaLog log);
        Task<List<Subasta>> ObtenerProgramadasParaActivarAsync(DateTime fechaActual);
        Task<List<Subasta>> ObtenerVencidasAsync(DateTime fechaActual);
    }
}