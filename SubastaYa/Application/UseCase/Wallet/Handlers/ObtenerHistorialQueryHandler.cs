using Application.Interfaces;
using Application.Mappings;
using Application.Models;
using Application.UseCases.Wallet.Queries;
using Domain.Exceptions;

namespace Application.UseCases.Wallet.Handlers
{
    public class ObtenerHistorialQueryHandler
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