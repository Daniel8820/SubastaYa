using Application.Interfaces;
using Application.UseCases.Wallet.Commands;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Wallet.Handlers
{
    public class DepositarFondosCommandHandler
    {
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DepositarFondosCommandHandler(IBilleteraRepository billeteraRepository, IUnitOfWork unitOfWork)
        {
            _billeteraRepository = billeteraRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> HandleAsync(DepositarFondosCommand command)
        {
            if (command.Monto <= 0) throw new DomainException("El monto a depositar debe ser mayor a cero.");

            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId);
            if (billetera == null) throw new DomainException("No se encontró una billetera asociada a este usuario.");

            billetera.SaldoTotal += command.Monto;
            billetera.SaldoDisponible += command.Monto;
            _billeteraRepository.Actualizar(billetera);

            _billeteraRepository.AgregarTransaccion(new TransaccionLedger
            {
                BilleteraId = billetera.Id,
                Tipo = "DEPOSITO",
                Monto = command.Monto,
                Fecha = DateTime.UtcNow
            });

            _billeteraRepository.AgregarAuditoria(new AuditoriaLog
            {
                Entidad = "BILLETERA",
                EntidadId = billetera.Id,
                Accion = "ACREDITACION_MANUAL",
                UsuarioId = command.UsuarioId,
                DetalleJson = $"{{ \"montoDepositado\": {command.Monto}, \"nuevoSaldo\": {billetera.SaldoDisponible} }}",
                Fecha = DateTime.UtcNow
            });

            // Impacta todo atómicamente en BD
            await _unitOfWork.SaveChangesAsync();

            return billetera.SaldoTotal;
        }
    }
}