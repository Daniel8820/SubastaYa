using Application.Interfaces;
using Application.Mappings;
using Application.Models;
using Application.UseCases.Subastas.Queries;

namespace Application.UseCases.Subastas.Handlers
{
    public class GetCatalogoSubastasQueryHandler
    {
        private readonly ISubastaRepository _subastaRepository;

        public GetCatalogoSubastasQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<CatalogoResponseDto> HandleAsync(GetCatalogoSubastasQuery query)
        {
            var resultado = await _subastaRepository.ObtenerCatalogoPaginadoAsync(
                query.Estado, query.CategoriaId, query.PrecioMin, query.PrecioMax,
                query.Orden, query.Pagina, query.TamañoPagina);

            return new CatalogoResponseDto
            {
                Paginacion = new PaginacionInfo
                {
                    TotalItems = resultado.TotalItems,
                    TotalPaginas = (int)Math.Ceiling(resultado.TotalItems / (double)query.TamañoPagina),
                    PaginaActual = query.Pagina,
                    TamañoPagina = query.TamañoPagina
                },
                // Magia: Mapeamos la lista de base de datos a DTOs
                Items = resultado.Items.Select(s => s.ToListItemDto()).ToList()
            };
        }
    }
}