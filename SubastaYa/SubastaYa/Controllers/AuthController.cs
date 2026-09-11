using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Usuarios.RegistrarUsuario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _config;

        public AuthController(
            IAuthService authService,
            IUsuarioRepository usuarioRepository,
            IConfiguration config)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // 1. Validamos credenciales mediante la infraestructura y obtenemos el IdentityId
                string identityId = await _authService.ValidarCredencialesAsync(request.Correo, request.Password);

                // 2. Buscamos el usuario en nuestra tabla de Dominio para obtener su ID numérico y su Nombre
                var usuarioDomain = await _usuarioRepository.ObtenerPorIdentityIdAsync(identityId);
                if (usuarioDomain == null)
                    return Unauthorized(new { error = "Perfil de usuario no encontrado en el sistema." });

                // 3. Armamos los Claims con los datos del dominio
                var jwtSettings = _config.GetSection("JwtSettings");
                var secretKey = jwtSettings.GetValue<string>("SecretKey");

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuarioDomain.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, usuarioDomain.Email ?? ""),
                    new Claim("nombre", usuarioDomain.Nombre),
                    new Claim(ClaimTypes.Role, "User")
                };

                // 4. Firmamos el token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new JwtSecurityToken(
                    issuer: jwtSettings.GetValue<string>("Issuer"),
                    audience: jwtSettings.GetValue<string>("Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationInMinutes")),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor),
                    expiracion = tokenDescriptor.ValidTo
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Correo o contraseña incorrectos." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegistrarUsuario(
            [FromBody] RegistrarUsuarioCommand command,
            [FromServices] RegistrarUsuarioCommandHandler handler)
        {
            int nuevoUsuarioId = await handler.HandleAsync(command);

            return Created(string.Empty, new
            {
                mensaje = "Usuario registrado exitosamente. Ya podés iniciar sesión.",
                usuarioId = nuevoUsuarioId
            });
        }
    }
}