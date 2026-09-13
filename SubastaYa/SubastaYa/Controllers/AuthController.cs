using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Usuarios.Login;
using SubastaYa.Application.UseCases.Usuarios.RegistrarUsuario;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            [FromServices] LoginCommandHandler handler)
        {
            try
            {
                var response = await handler.HandleAsync(command);
                return Ok(response);
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