using Application.UseCases.Usuarios.Commands;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Usuarios.Handlers
{
    public class RegistrarUsuarioCommandHandler
    {
        private readonly UserManager<Usuario> _userManager;

        // Inyectamos el motor de Identity
        public RegistrarUsuarioCommandHandler(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<int> HandleAsync(RegistrarUsuarioCommand command)
        {
            // 1. Sanitizamos las entradas: eliminamos espacios invisibles al principio y al final
            command.Nombre = command.Nombre;
            command.Email = command.Email?.Trim();
            command.Password = command.Password?.Trim();

            // 2. Validamos que, una vez limpios, no hayan quedado vacíos
            if (string.IsNullOrWhiteSpace(command.Nombre) ||
                string.IsNullOrWhiteSpace(command.Email) ||
                string.IsNullOrWhiteSpace(command.Password))
            {
                throw new DomainException("El nombre, el email y la contraseña son obligatorios y no pueden ser solo espacios.");
            }

            // ... opcional: si querés prohibir estrictamente los espacios INCLUSO en el medio de la clave:
            if (command.Password.Contains(" "))
            {
                throw new DomainException("La contraseña no puede contener espacios en blanco.");
            }

            var nuevoUsuario = new Usuario
            {
                UserName = command.Email, // Identity exige que UserName tenga un valor (usamos el email)
                Email = command.Email,
                Nombre = command.Nombre,
                FechaRegistro = DateTime.UtcNow,

                // Creamos la billetera vinculada en el mismo acto
                Billetera = new Billetera
                {
                    SaldoTotal = 0,
                    SaldoDisponible = 0,
                    SaldoRetenido = 0,
                    Version = 1
                }
            };

            // Magia: CreateAsync encripta la clave "123456" y lo guarda en la BD junto a su billetera.
            var resultado = await _userManager.CreateAsync(nuevoUsuario, command.Password);

            if (!resultado.Succeeded)
            {
                // Si el mail ya existe o la clave es débil, Identity nos avisa acá
                var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                throw new DomainException($"No se pudo registrar el usuario: {errores}");
            }

            return nuevoUsuario.Id;
        }
    }
}