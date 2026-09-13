using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Usuarios.ActualizarPerfil;
using SubastaYa.Application.UseCases.Usuarios.CambiarPassword;
using SubastaYa.Application.UseCases.Usuarios.GetMisActividades;
using SubastaYa.Application.Interfaces.Services;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        public UsuariosController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        [HttpGet("me/activities")]
        public async Task<IActionResult> MisActividades([FromServices] GetMisActividadesQueryHandler handler)
        {
            var query = new GetMisActividadesQuery { UsuarioId = _currentUser.UsuarioId };
            var resultado = await handler.HandleAsync(query);
            return Ok(resultado);
        }

        [HttpPut("me/profile")]
        public async Task<IActionResult> ActualizarPerfil(
            [FromBody] ActualizarPerfilCommand command,
            [FromServices] ActualizarPerfilCommandHandler handler)
        {
            command.UsuarioId = _currentUser.UsuarioId;
            await handler.HandleAsync(command);
            return Ok(new { mensaje = "Perfil actualizado exitosamente." });
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> CambiarPassword(
            [FromBody] CambiarPasswordCommand command,
            [FromServices] CambiarPasswordCommandHandler handler)
        {
            command.UsuarioId = _currentUser.UsuarioId;
            await handler.HandleAsync(command);
            return Ok(new { mensaje = "Contraseña cambiada exitosamente." });
        }
    }
}