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

            if (string.IsNullOrWhiteSpace(command.NuevoNombre))
                throw new DomainException("El nombre no puede estar vacío o contener solo espacios.");

            // Traemos la entidad pura
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(command.UsuarioId);

            if (usuario == null)
                throw new DomainException("Usuario no encontrado.");

            // Modificamos los datos de negocio
            usuario.Nombre = command.NuevoNombre;

            // Persistimos los cambios
            _usuarioRepository.Actualizar(usuario);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}