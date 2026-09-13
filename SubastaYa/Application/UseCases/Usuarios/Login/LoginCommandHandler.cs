using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Usuarios.Login
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;

        public LoginCommandHandler(IAuthService authService, IUsuarioRepository usuarioRepository)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<LoginResponseDto> HandleAsync(LoginCommand command)
        {
            string identityId = await _authService.ValidarCredencialesAsync(command.Correo, command.Password);

            var usuarioDomain = await _usuarioRepository.ObtenerPorIdentityIdAsync(identityId);
            if (usuarioDomain == null)
                throw new DomainException("Perfil de usuario no encontrado en el sistema.");

            string tokenStr = await _authService.GenerarTokenJwtAsync(usuarioDomain);

            return new LoginResponseDto
            {
                Token = tokenStr,
                Expiracion = DateTime.UtcNow.AddMinutes(120)
            };
        }
    }
}