using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.DTOs;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Wallet.ConsultarSaldo
{
    public class ConsultarSaldoQueryHandler : IQueryHandler<ConsultarSaldoQuery, SaldoResponseDto>
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