using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.UseCases.Subastas.FinalizarSubastas
{
    public class FinalizarSubastasCommandHandler : ICommandHandler<FinalizarSubastasCommand, int>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FinalizarSubastasCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(FinalizarSubastasCommand command)
        {
            
            var subastasVencidas = await _subastaRepository.ObtenerVencidasAsync(DateTime.UtcNow);

            if (!subastasVencidas.Any()) return 0;

            foreach (var subasta in subastasVencidas)
            {
                var pujaGanadora = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
                string estadoAnterior = subasta.Estado;
                string nuevoEstado;

                if (pujaGanadora != null)
                {
                    var billeteraVendedor = await _billeteraRepository.ObtenerPorUsuarioIdAsync(subasta.VendedorId);
                    if (billeteraVendedor != null)
                    {
                        // Reemplazamos la suma manual por el método de dominio
                        billeteraVendedor.Acreditar(pujaGanadora.Monto);
                        _billeteraRepository.Actualizar(billeteraVendedor);

                        _billeteraRepository.AgregarTransaccion(new TransaccionLedger
                        {
                            BilleteraId = billeteraVendedor.Id,
                            Tipo = "ACREDITACION_VENTA",
                            Monto = pujaGanadora.Monto,
                            Fecha = DateTime.UtcNow,
                            SubastaId = subasta.Id
                        });
                    }

                    var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadora.CompradorId);
                    if (billeteraComprador != null)
                    {
                        // Reemplazamos la resta manual por el método de dominio
                        billeteraComprador.DescontarPago(pujaGanadora.Monto);
                        _billeteraRepository.Actualizar(billeteraComprador);

                        _billeteraRepository.AgregarTransaccion(new TransaccionLedger
                        {
                            BilleteraId = billeteraComprador.Id,
                            Tipo = "PAGO_SUBASTA",
                            Monto = pujaGanadora.Monto,
                            Fecha = DateTime.UtcNow,
                            SubastaId = subasta.Id
                        });
                    }
                    nuevoEstado = EstadosSubasta.Finalizada;
                }
                else
                {
                    nuevoEstado = EstadosSubasta.Desierta;
                }

                subasta.Estado = nuevoEstado;
                _subastaRepository.Actualizar(subasta);

                _subastaRepository.AgregarAuditoria(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "CAMBIO_ESTADO",
                    UsuarioId = null,
                    DetalleJson = $"{{ \"estadoAnterior\": \"{estadoAnterior}\", \"nuevoEstado\": \"{nuevoEstado}\" }}",
                    Fecha = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return subastasVencidas.Count;
        }
    }
}