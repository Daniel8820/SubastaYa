using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.DTOs;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Wallet.ObtenerHistorial
{
    public class ObtenerHistorialQueryHandler : IQueryHandler<ObtenerHistorialQuery, List<TransaccionDto>>
    {
        private readonly IBilleteraRepository _billeteraRepository;

        public ObtenerHistorialQueryHandler(IBilleteraRepository billeteraRepository)
        {
            _billeteraRepository = billeteraRepository;
        }

        public async Task<List<TransaccionDto>> HandleAsync(ObtenerHistorialQuery query)
        {
            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId);
            if (billetera == null) throw new DomainException("Billetera no encontrada.");

            var historial = await _billeteraRepository.ObtenerHistorialAsync(billetera.Id);

            return historial.Select(t => t.ToDto()).ToList();
        }
    }
}