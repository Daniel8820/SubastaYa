using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Usuarios.CambiarPassword
{
    public class CambiarPasswordCommandHandler : ICommandHandler<CambiarPasswordCommand>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuthService _authService;

        public CambiarPasswordCommandHandler(
            IUsuarioRepository usuarioRepository,
            IAuthService authService)
        {
            _usuarioRepository = usuarioRepository;
            _authService = authService;
        }

        public async Task HandleAsync(CambiarPasswordCommand command)
        {
            // 1. Buscamos nuestro usuario del Dominio usando el ID que viene en el token (UsuarioId)
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(command.UsuarioId);

            if (usuario == null)
                throw new DomainException("Usuario no encontrado.");

            // 2. Usamos su vínculo (IdentityId) para decirle a Infraestructura que cambie la clave
            await _authService.CambiarPasswordAsync(
                usuario.IdentityId,
                command.PasswordActual,
                command.PasswordNueva);
        }
    }
}