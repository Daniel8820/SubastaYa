using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface ISubastaRepository
    {
        Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task AgregarAsync(Subasta subasta, CancellationToken ct = default);
        void Actualizar(Subasta subasta);
        Task<List<Subasta>> ObtenerPorVendedorIdAsync(int vendedorId, CancellationToken ct = default);
        Task<List<Subasta>> ObtenerPorCompradorIdAsync(int compradorId, CancellationToken ct = default);
        Task<Subasta?> ObtenerDetallePorIdAsync(int id, CancellationToken ct = default);
        Task<(List<Subasta> Items, int TotalItems)> ObtenerCatalogoPaginadoAsync(string estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string orden, int pagina, int tamañoPagina, CancellationToken ct = default);
        void AgregarAuditoria(AuditoriaLog log);
        Task<List<Subasta>> ObtenerProgramadasParaActivarAsync(DateTime fechaActual, CancellationToken ct = default);
        Task<List<Subasta>> ObtenerVencidasAsync(DateTime fechaActual, CancellationToken ct = default);
    }
}