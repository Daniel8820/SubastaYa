using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.RegistrarPuja
{
    public class RegistrarPujaCommandHandler : ICommandHandler<RegistrarPujaCommand, bool>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificadorSubastas _notificador;

        public RegistrarPujaCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IUnitOfWork unitOfWork,
            INotificadorSubastas notificador)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _unitOfWork = unitOfWork;
            _notificador = notificador;
        }

        public async Task<bool> HandleAsync(RegistrarPujaCommand command)
        {
            var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId);
            if (subasta == null) throw new DomainException("La subasta no existe.");

            var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId);
            if (billeteraComprador == null) throw new DomainException("El comprador no tiene una billetera asociada.");

            var pujaGanadoraAnterior = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
            var fechaFinAnterior = subasta.FechaFin;

            var nuevaPuja = new Puja
            {
                SubastaId = subasta.Id,
                CompradorId = command.CompradorId,
                Monto = command.Monto,
                FechaPuja = DateTime.UtcNow
            };

            // Delegamos la lógica al Dominio
            bool tiempoExtendido = subasta.ProcesarPuja(nuevaPuja);

            // Efectuamos compensaciones financieras
            billeteraComprador.RetenerFondos(command.Monto);
            _billeteraRepository.Actualizar(billeteraComprador);

            if (pujaGanadoraAnterior != null)
            {
                var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadoraAnterior.CompradorId);
                if (billeteraAnterior != null)
                {
                    billeteraAnterior.LiberarGarantia(pujaGanadoraAnterior.Monto);
                    _billeteraRepository.Actualizar(billeteraAnterior);
                }
            }

            if (tiempoExtendido)
            {
                _subastaRepository.AgregarAuditoria(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "EXTENSION_TIEMPO_ANTISNIPING",
                    UsuarioId = command.CompradorId,
                    DetalleJson = $"{{ \"fechaFinAnterior\": \"{fechaFinAnterior:O}\", \"nuevaFechaFin\": \"{subasta.FechaFin:O}\" }}",
                    Fecha = DateTime.UtcNow
                });
            }

            _subastaRepository.Actualizar(subasta);

            try
            {
                await _unitOfWork.SaveChangesAsync();

                await _notificador.NotificarNuevaPujaAsync(
                    command.SubastaId,
                    command.Monto,
                    command.CompradorNombre,
                    command.CompradorId);

                return true;
            }
            catch (ConcurrencyDomainException) // Atrapamos el error abstracto del dominio
            {
                return false;
            }
        }
    }
}