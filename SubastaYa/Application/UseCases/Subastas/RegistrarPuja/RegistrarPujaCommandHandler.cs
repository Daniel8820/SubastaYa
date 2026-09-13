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

            if (subasta.Estado != EstadosSubasta.Activa || subasta.FechaFin <= DateTime.UtcNow)
                throw new DomainException("La subasta ya ha finalizado o no se encuentra activa.");

            if (subasta.VendedorId == command.CompradorId)
                throw new DomainException("No puedes pujar en una subasta que tú mismo has publicado.");

            // 1. Regla Anti Auto-Puja
            var pujaGanadoraActual = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
            if (pujaGanadoraActual != null && pujaGanadoraActual.CompradorId == command.CompradorId)
                throw new DomainException("Ya posees la oferta más alta en esta subasta. No puedes pujar contra ti mismo.");

            // 2. Validación de Monto Blindada
            bool esPrimeraPuja = !subasta.Pujas.Any();
            var ofertaMasAlta = esPrimeraPuja ? subasta.PrecioBase : subasta.Pujas.Max(p => p.Monto);

            var montoMinimoRequerido = esPrimeraPuja && command.Monto == subasta.PrecioBase
                ? subasta.PrecioBase
                : ofertaMasAlta + subasta.IncrementoMinimo;

            if (command.Monto < montoMinimoRequerido)
            {
                if (esPrimeraPuja)
                    throw new DomainException($"La primera oferta debe ser exactamente el Precio Base (${subasta.PrecioBase}) o un mínimo de ${subasta.PrecioBase + subasta.IncrementoMinimo}.");
                else
                    throw new DomainException($"El monto es inválido. Debe superar la oferta actual por al menos ${subasta.IncrementoMinimo} (Mínimo: ${montoMinimoRequerido}).");
            }

            // 3. Garantía y Billetera
            var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId);
            if (billeteraComprador == null) throw new DomainException("El comprador no tiene una billetera asociada.");

            billeteraComprador.RetenerFondos(command.Monto);
            _billeteraRepository.Actualizar(billeteraComprador);

            if (pujaGanadoraActual != null)
            {
                var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadoraActual.CompradorId);
                if (billeteraAnterior != null)
                {
                    billeteraAnterior.LiberarGarantia(pujaGanadoraActual.Monto);
                    _billeteraRepository.Actualizar(billeteraAnterior);
                }
            }

            // 4. Regla Anti-Sniping con Auditoría
            var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;
            if (tiempoRestante.TotalSeconds > 0 && tiempoRestante.TotalSeconds <= 60)
            {
                var fechaFinAnterior = subasta.FechaFin;
                subasta.FechaFin = subasta.FechaFin.AddMinutes(2);

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

            // 5. <--- EL BLOQUE PERDIDO: Registrar la nueva Puja --->
            subasta.Pujas.Add(new Puja
            {
                SubastaId = subasta.Id,
                CompradorId = command.CompradorId,
                Monto = command.Monto,
                FechaPuja = DateTime.UtcNow
            });

            subasta.Version++;
            _subastaRepository.Actualizar(subasta);

            try
            {
                await _unitOfWork.SaveChangesAsync();

                // Disparamos el WebSocket con todos los datos
                await _notificador.NotificarNuevaPujaAsync(
                    command.SubastaId,
                    command.Monto,
                    command.CompradorNombre,
                    command.CompradorId);

                return true;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}