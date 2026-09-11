using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Usuarios.ActualizarPerfil
{
    public class ActualizarPerfilCommandHandler : ICommandHandler<ActualizarPerfilCommand>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActualizarPerfilCommandHandler(
            IUsuarioRepository usuarioRepository,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ActualizarPerfilCommand command)
        {
            // 1. Traemos la entidad pura
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(command.UsuarioId);

            if (usuario == null)
                throw new DomainException("Usuario no encontrado.");

            // 2. Modificamos los datos de negocio
            usuario.Nombre = command.NuevoNombre;

            // 3. Persistimos los cambios
            _usuarioRepository.Actualizar(usuario);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}