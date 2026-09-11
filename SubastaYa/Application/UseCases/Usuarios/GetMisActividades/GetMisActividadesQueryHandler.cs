using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.UseCases.Usuarios.GetMisActividades
{
    public class GetMisActividadesQueryHandler : IQueryHandler<GetMisActividadesQuery, MisActividadesResponseDto>
    {
        private readonly ISubastaRepository _subastaRepository;

        public GetMisActividadesQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<MisActividadesResponseDto> HandleAsync(GetMisActividadesQuery query)
        {
            var publicaciones = await _subastaRepository.ObtenerPorVendedorIdAsync(query.UsuarioId);
            var pujas = await _subastaRepository.ObtenerPorCompradorIdAsync(query.UsuarioId);

            var response = new MisActividadesResponseDto();

            response.MisPublicaciones = publicaciones.Select(s => s.ToPublicacionDto()).ToList();
            response.MisComprasYPujas = pujas.Select(s => s.ToParticipacionDto(query.UsuarioId)).ToList();

            return response;
        }
    }
}