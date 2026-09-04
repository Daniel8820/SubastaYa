using Application.Interfaces;
using Application.UseCases.Subastas.Commands;
using Domain.Exceptions;

namespace Application.UseCases.Subastas.Handlers
{
    public class RegistrarPujaCommandHandler
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarPujaCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _unitOfWork = unitOfWork;
        }

        // Ahora recibe el Command que viene directo del Controller
        public async Task<bool> HandleAsync(RegistrarPujaCommand command)
        {
            var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId);
            if (subasta == null)
                throw new DomainException("La subasta no existe.");

            if (subasta.Estado != "ACTIVA" || subasta.FechaFin <= DateTime.UtcNow)
                throw new DomainException("La subasta ya ha finalizado o no se encuentra activa.");

            // 1. Validar el monto de la oferta y el incremento mínimo
            var ofertaMasAlta = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase;
            var montoMinimoRequerido = subasta.Pujas.Any() ? ofertaMasAlta + subasta.IncrementoMinimo : subasta.PrecioBase;

            if (command.Monto < montoMinimoRequerido)
            {
                throw new DomainException($"El monto de la puja es inválido. Debe ser de al menos ${montoMinimoRequerido}.");
            }

            var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId);
            if (billeteraComprador == null)
                throw new DomainException("El comprador no tiene una billetera asociada.");

            // 2. Validar saldo disponible
            if (billeteraComprador.SaldoDisponible < command.Monto)
            {
                throw new DomainException("Saldo insuficiente para realizar esta puja.");
            }

            // 3. Lógica de Escrow (Garantía) - Liberar anterior y retener nuevo
            var pujaAnterior = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
            if (pujaAnterior != null)
            {
                var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaAnterior.CompradorId);
                if (billeteraAnterior != null)
                {
                    billeteraAnterior.SaldoRetenido -= pujaAnterior.Monto;
                    billeteraAnterior.SaldoDisponible += pujaAnterior.Monto;
                    _billeteraRepository.Actualizar(billeteraAnterior);
                }
            }

            billeteraComprador.SaldoDisponible -= command.Monto;
            billeteraComprador.SaldoRetenido += command.Monto;

            _billeteraRepository.Actualizar(billeteraComprador);

            // 4. Regla Anti-Sniping
            var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;
            if (tiempoRestante.TotalSeconds > 0 && tiempoRestante.TotalSeconds <= 60)
            {
                subasta.FechaFin = subasta.FechaFin.AddMinutes(2);
            }

            // 5. Registrar la nueva Puja en la subasta
            subasta.Pujas.Add(new Domain.Entities.Puja
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
                // Guardamos todo de forma atómica usando el Unit of Work
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                // Si hay choque de concurrencia optimista, devolvemos false para que el controller tire el 409
                return false;
            }
        }
    }
}