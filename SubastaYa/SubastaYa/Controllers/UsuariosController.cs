using Application.UseCases.Usuarios.Queries;
using Application.UseCases.Usuarios.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SubastaYa.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        // Ya no inyectamos el DbContext
        public UsuariosController()
        {
        }

        private int ObtenerUsuarioIdDelToken()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(claimId))
            {
                throw new System.Exception("No se pudo extraer el ID del usuario desde el token.");
            }

            return int.Parse(claimId);
        }

        [HttpGet("me/activities")]
        public async Task<IActionResult> MisActividades(
            [FromServices] GetMisActividadesQueryHandler handler) // Inyectamos el Handler de CQRS
        {
            int usuarioId = ObtenerUsuarioIdDelToken();

            // Armamos la Query y se la pasamos al Handler
            var query = new GetMisActividadesQuery { UsuarioId = usuarioId };
            var resultado = await handler.HandleAsync(query);

            return Ok(resultado);
        }
    }
}