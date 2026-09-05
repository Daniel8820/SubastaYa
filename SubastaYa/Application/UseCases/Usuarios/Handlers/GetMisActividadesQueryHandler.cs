using Application.Interfaces;
using Application.Mappings;
using Application.Models;
using Application.UseCases.Usuarios.Queries;

namespace Application.UseCases.Usuarios.Handlers
{
    public class GetMisActividadesQueryHandler
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