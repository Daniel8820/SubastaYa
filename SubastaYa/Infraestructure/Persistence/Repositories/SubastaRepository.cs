using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SubastaRepository : ISubastaRepository
    {
        private readonly SubastaYaDbContext _context;

        public SubastaRepository(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<Subasta?> ObtenerPorIdAsync(int id)
        {
            return await _context.Subastas.FindAsync(id);
        }

        public async Task AgregarAsync(Subasta subasta)
        {
            await _context.Subastas.AddAsync(subasta);
        }

        public void Actualizar(Subasta subasta)
        {
            _context.Subastas.Update(subasta);
        }

        public async Task<List<Domain.Entities.Subasta>> ObtenerPorVendedorIdAsync(int vendedorId)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.VendedorId == vendedorId)
                .ToListAsync();
        }

        public async Task<List<Domain.Entities.Subasta>> ObtenerPorCompradorIdAsync(int compradorId)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.Pujas.Any(p => p.CompradorId == compradorId))
                .ToListAsync();
        }

        // --- NUEVOS MÉTODOS PARA LOS GET ---

        public async Task<Subasta?> ObtenerDetallePorIdAsync(int id)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Comprador)
                .Include(s => s.Vendedor)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<(List<Domain.Entities.Subasta> Items, int TotalItems)> ObtenerCatalogoPaginadoAsync(
            string estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string orden, int pagina, int tamañoPagina)
        {
            var query = _context.Subastas
                .Include(s => s.Pujas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(s => s.Estado.ToUpper() == estado.ToUpper());

            if (categoriaId.HasValue)
                query = query.Where(s => s.CategoriaId == categoriaId.Value);

            if (precioMin.HasValue)
                query = query.Where(s => (s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase) >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(s => (s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase) <= precioMax.Value);

            if (orden?.ToLower() == "mayor_puja")
                query = query.OrderByDescending(s => s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase);
            else
                query = query.OrderBy(s => s.FechaFin);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            return (items, totalItems);
        }
    }
}