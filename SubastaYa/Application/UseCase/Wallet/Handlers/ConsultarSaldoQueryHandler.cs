using Application.Interfaces;
using Application.Mappings;
using Application.Models;
using Application.UseCases.Wallet.Queries;
using Domain.Exceptions;

namespace Application.UseCases.Wallet.Handlers
{
    public class ConsultarSaldoQueryHandler
    {
        private readonly IBilleteraRepository _billeteraRepository;

        public ConsultarSaldoQueryHandler(IBilleteraRepository billeteraRepository)
        {
            _billeteraRepository = billeteraRepository;
        }

        public async Task<SaldoResponseDto> HandleAsync(ConsultarSaldoQuery query)
        {
            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId);
            if (billetera == null) throw new DomainException("No se encontró una billetera asociada a este usuario.");

            return billetera.ToSaldoDto();
        }
    }
}