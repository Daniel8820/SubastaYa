using Application.UseCases.Usuarios.Commands;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Usuarios.Handlers
{
    public class CambiarPasswordCommandHandler
    {
        private readonly UserManager<Usuario> _userManager;

        public CambiarPasswordCommandHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> HandleAsync(CambiarPasswordCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.PasswordActual) || string.IsNullOrWhiteSpace(command.PasswordNueva))
                throw new DomainException("Las contraseñas no pueden estar vacías.");

            if (command.PasswordNueva.Contains(" "))
                throw new DomainException("La nueva contraseña no puede contener espacios en blanco.");

            var usuario = await _userManager.FindByIdAsync(command.UsuarioId.ToString());

            if (usuario == null)
                throw new DomainException("Usuario no encontrado.");

            // Identity se encarga de validar la clave actual, hashear la nueva y guardarla de forma segura
            var resultado = await _userManager.ChangePasswordAsync(usuario, command.PasswordActual, command.PasswordNueva);

            if (!resultado.Succeeded)
            {
                // Si la clave no cumple los requisitos (mayúsculas, números, etc.) o la actual es incorrecta
                var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                throw new DomainException($"No se pudo cambiar la contraseña: {errores}");
            }

            return true;
        }
    }
}