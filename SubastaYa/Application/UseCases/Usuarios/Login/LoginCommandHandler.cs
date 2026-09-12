using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SubastaYa.Application.UseCases.Usuarios.Login
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _config;

        public LoginCommandHandler(IAuthService authService, IUsuarioRepository usuarioRepository, IConfiguration config)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
            _config = config;
        }

        public async Task<LoginResponseDto> HandleAsync(LoginCommand command)
        {
            string identityId = await _authService.ValidarCredencialesAsync(command.Correo, command.Password);

            var usuarioDomain = await _usuarioRepository.ObtenerPorIdentityIdAsync(identityId);
            if (usuarioDomain == null)
                throw new DomainException("Perfil de usuario no encontrado en el sistema.");

            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioDomain.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuarioDomain.Email ?? ""),
                new Claim("nombre", usuarioDomain.Nombre),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtSettings.GetValue<string>("Issuer"),
                audience: jwtSettings.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationInMinutes")),
                signingCredentials: creds
            );

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor),
                Expiracion = tokenDescriptor.ValidTo
            };
        }
    }
}