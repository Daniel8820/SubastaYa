using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.UseCases.Subastas.ActivarSubastas
{
    public class ActivarSubastasCommandHandler : ICommandHandler<ActivarSubastasCommand, int>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivarSubastasCommandHandler(ISubastaRepository subastaRepository, IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(ActivarSubastasCommand command)
        {
            var subastasParaActivar = await _subastaRepository.ObtenerProgramadasParaActivarAsync(DateTime.UtcNow);

            if (!subastasParaActivar.Any()) return 0;

            foreach (var subasta in subastasParaActivar)
            {
                subasta.Estado = EstadosSubasta.Activa;
                _subastaRepository.Actualizar(subasta);

                _subastaRepository.AgregarAuditoria(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "ACTIVACION_AUTOMATICA",
                    UsuarioId = null,
                    DetalleJson = $"{{ \"estadoAnterior\": \"{EstadosSubasta.Programada}\", \"nuevoEstado\": \"{EstadosSubasta.Activa}\" }}",
                    Fecha = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return subastasParaActivar.Count;
        }
    }
}