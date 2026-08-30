using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Application.Models;

namespace SubastaYa.Presentacion.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SubastaYaDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(SubastaYaDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Buscamos al usuario en la base de datos (por simplicidad, sin encriptar la clave por ahora)
            // Ajuste: la entidad Usuario usa Email y PasswordHash
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Correo && u.PasswordHash == request.Password);

            if (usuario == null)
            {
                return Unauthorized(new { error = "Correo o contraseña incorrectos." });
            }

            // 2. Si el usuario existe, armamos su "pasaporte" (Claims)
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("nombre", usuario.Nombre),
                // La entidad Usuario actual no define un rol; por ahora devolvemos 'User' por defecto
                new Claim(ClaimTypes.Role, "User")
            };

            // 3. Firmamos el token con nuestra llave maestra
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.GetValue<string>("Issuer"),
                audience: jwtSettings.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationInMinutes")),
                signingCredentials: creds
            );

            // 4. Devolvemos el token serializado
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiracion = token.ValidTo
            });
        }
    }
}