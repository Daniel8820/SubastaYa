using Application.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SubastaYa.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _config;

        // Inyectamos el motor de Identity y la configuración del JWT
        public AuthController(UserManager<Usuario> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Buscamos al usuario por su email
            var usuario = await _userManager.FindByEmailAsync(request.Correo);

            // 2. Comparamos la contraseña de forma segura usando el hash de la base de datos
            if (usuario == null || !await _userManager.CheckPasswordAsync(usuario, request.Password))
            {
                return Unauthorized(new { error = "Correo o contraseña incorrectos." });
            }

            // 3. Armamos los Claims (el pasaporte del usuario)
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? ""),
                new Claim("nombre", usuario.Nombre),
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

        [HttpPost("register")]
        public async Task<IActionResult> RegistrarUsuario(
            [FromBody] Application.UseCases.Usuarios.Commands.RegistrarUsuarioCommand command,
            [FromServices] Application.UseCases.Usuarios.Handlers.RegistrarUsuarioCommandHandler handler)
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