using SubastaYa.Domain.Exceptions;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Infrastructure.Identity;
using SubastaYa.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SubastaYa.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public async Task<string> RegistrarLoginAsync(string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new DomainException($"Error al crear login: {string.Join(" | ", result.Errors.Select(e => e.Description))}");
            return user.Id;
        }

        public async Task<bool> CambiarPasswordAsync(string identityId, string passwordActual, string nuevaPassword)
        {
            var user = await _userManager.FindByIdAsync(identityId);
            if (user == null) return false;
            var result = await _userManager.ChangePasswordAsync(user, passwordActual, nuevaPassword);
            if (!result.Succeeded)
                throw new DomainException($"Error al cambiar contraseña: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return true;
        }

        public async Task<string> ValidarCredencialesAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");
            return user.Id;
        }

        public Task<string> GenerarTokenJwtAsync(Usuario usuario)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? ""),
                new Claim("nombre", string.IsNullOrWhiteSpace(usuario.Nombre) ? "Anónimo" : usuario.Nombre),
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

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(tokenDescriptor));
        }
    }
}