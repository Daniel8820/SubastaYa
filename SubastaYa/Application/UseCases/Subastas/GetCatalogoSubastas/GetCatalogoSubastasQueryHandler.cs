using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Mappings;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.GetCatalogoSubastas
{
    public class GetCatalogoSubastasQueryHandler : IQueryHandler<GetCatalogoSubastasQuery, CatalogoResponseDto>
    {
        private readonly ISubastaRepository _subastaRepository;

        public GetCatalogoSubastasQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<CatalogoResponseDto> HandleAsync(GetCatalogoSubastasQuery query)
        {
            if (query.Pagina < 1)
                throw new DomainException("El número de página debe ser mayor a cero.");

            if (query.TamañoPagina < 1)
                throw new DomainException("El tamaño de la página debe ser mayor a cero.");
            
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
                // Mapeamos la lista de base de datos a DTOs
                Items = resultado.Items.Select(s => s.ToListItemDto()).ToList()
            };
        }
    }
}