using Application.UseCases.Usuarios.Commands;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Usuarios.Handlers
{
    public class ActualizarPerfilCommandHandler
    {
        private readonly UserManager<Usuario> _userManager;

        public ActualizarPerfilCommandHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> HandleAsync(ActualizarPerfilCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.NuevoNombre))
                throw new DomainException("El nombre no puede estar vacío.");

            var usuario = await _userManager.FindByIdAsync(command.UsuarioId.ToString());

            if (usuario == null)
                throw new DomainException("Usuario no encontrado.");

            usuario.Nombre = command.NuevoNombre.Trim();

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
                throw new DomainException("Hubo un error al actualizar el perfil.");

            return true;
        }
    }
}