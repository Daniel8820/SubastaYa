using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.UseCases.Subastas.GetSubastaById
{
    public class GetSubastaByIdQueryHandler : IQueryHandler<GetSubastaByIdQuery, SubastaDetalleResponseDto?>
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