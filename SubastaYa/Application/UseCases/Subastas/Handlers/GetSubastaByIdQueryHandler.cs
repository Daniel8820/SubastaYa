using Application.Interfaces;
using Application.Mappings;
using Application.Models;
using Application.UseCases.Subastas.Queries;

namespace Application.UseCases.Subastas.Handlers
{
    public class GetSubastaByIdQueryHandler
    {
        private readonly ISubastaRepository _subastaRepository;

        public GetSubastaByIdQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<SubastaDetalleResponseDto?> HandleAsync(GetSubastaByIdQuery query)
        {
            var subasta = await _subastaRepository.ObtenerDetallePorIdAsync(query.Id);
            if (subasta == null) return null;

            return subasta.ToDetalleDto(); // Usamos nuestro mapper limpio
        }
    }
}