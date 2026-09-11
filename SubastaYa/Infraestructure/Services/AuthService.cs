using SubastaYa.Domain.Exceptions;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace SubastaYa.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> RegistrarLoginAsync(string email, string password)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));

                throw new DomainException($"Error al crear login: {errors}");
            }

            return user.Id;
        }

        public async Task<bool> CambiarPasswordAsync(string identityId, string passwordActual, string nuevaPassword)
        {
            // Buscamos al usuario de Identity por su ID interno
            var user = await _userManager.FindByIdAsync(identityId);
            if (user == null) return false;

            // Identity se encarga de verificar el hash actual y generar el nuevo
            var result = await _userManager.ChangePasswordAsync(user, passwordActual, nuevaPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al cambiar contraseña: {errors}");
            }

            return true;
        }

        public async Task<string> ValidarCredencialesAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");
            }

            return user.Id;
        }
    }
}