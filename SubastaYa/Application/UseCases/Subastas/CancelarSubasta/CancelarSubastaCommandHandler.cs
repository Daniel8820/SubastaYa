using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.CancelarSubasta
{
    public class CancelarSubastaCommandHandler : ICommandHandler<CancelarSubastaCommand, bool>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelarSubastaCommandHandler(ISubastaRepository subastaRepository, IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> HandleAsync(CancelarSubastaCommand command)
        {
            var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId);

            if (subasta == null)
                throw new DomainException("La subasta no existe.");

            if (subasta.VendedorId != command.VendedorId)
                throw new DomainException("Solo el vendedor original puede cancelar esta subasta.");

            if (subasta.Estado != "ACTIVA" && subasta.Estado != "PROGRAMADA")
                throw new DomainException("Solo se pueden cancelar subastas activas o programadas.");

            // Regla de negocio - Cancelacion
            if (subasta.Pujas.Any())
                throw new DomainException("No se puede cancelar una subasta que ya posee ofertas registradas.");

            // Guardamos el estado anterior para el log
            string estadoAnterior = subasta.Estado;

            // Cambiamos el estado
            subasta.Estado = "CANCELADA";
            _subastaRepository.Actualizar(subasta);

            // Registro de Auditoría
            _subastaRepository.AgregarAuditoria(new Domain.Entities.AuditoriaLog
            {
                Entidad = "SUBASTA",
                EntidadId = subasta.Id,
                Accion = "CANCELACION_MANUAL",
                UsuarioId = command.VendedorId, // Quién ejecutó la acción
                DetalleJson = $"{{ \"estadoAnterior\": \"{estadoAnterior}\", \"nuevoEstado\": \"CANCELADA\" }}",
                Fecha = DateTime.UtcNow
            });

            // Impactamos todo en la base de datos atómicamente
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}