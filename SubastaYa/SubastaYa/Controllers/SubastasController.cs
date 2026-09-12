using SubastaYa.Application.UseCases.Subastas.CancelarSubasta;
using SubastaYa.Application.UseCases.Subastas.CrearSubasta;
using SubastaYa.Application.UseCases.Subastas.GetCatalogoSubastas;
using SubastaYa.Application.UseCases.Subastas.GetSubastaById;
using SubastaYa.Application.UseCases.Subastas.RegistrarPuja;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class SubastasController : ControllerBase
    {
        public SubastasController()
        {
        }

        [Authorize]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> RegistrarPuja(
            int id,
            [FromBody] RegistrarPujaCommand command,
            [FromServices] RegistrarPujaCommandHandler handler)
        {
            // Extraemos el ID directamente del token de forma segura
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var claimNombre = User.FindFirst("nombre")?.Value ?? "Anónimo";

            if (string.IsNullOrEmpty(claimId))
                return Unauthorized(new { error = "Token inválido o sin permisos." });

            // Le inyectamos el ID real al comando
            command.CompradorId = int.Parse(claimId);
            command.CompradorNombre = claimNombre;
            command.SubastaId = id;

            // Ejecutamos la lógica de negocio
            bool resultado = await handler.HandleAsync(command);

            if (!resultado)
            {
                return Conflict(new { error = "Rechazo por concurrencia. Otro usuario acaba de pujar, por favor actualizá la subasta e intentá nuevamente." });
            }

            return Ok(new { mensaje = "Puja registrada exitosamente. Saldo retenido temporalmente." });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CrearSubasta(
            [FromBody] CrearSubastaCommand command,
            [FromServices] CrearSubastaCommandHandler handler)
        {
            // Extraemos el ID directamente del token
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(claimId))
                return Unauthorized(new { error = "Token inválido o sin permisos." });

            // 2. Le inyectamos el ID real del vendedor al comando
            command.VendedorId = int.Parse(claimId);

            int subastaId = await handler.HandleAsync(command);

            // 3. Devolvemos 201 Created
            return CreatedAtAction(
                nameof(ObtenerDetalleSubasta),
                new { id = subastaId },
                new { mensaje = "Subasta creada exitosamente", subastaId = subastaId }
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerDetalleSubasta(
            int id,
            [FromServices] GetSubastaByIdQueryHandler handler)
        {
            var query = new GetSubastaByIdQuery { Id = id };
            var response = await handler.HandleAsync(query);

            if (response == null)
            {
                return NotFound(new { error = "La subasta solicitada no existe." });
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCatalogo(
            [FromServices] GetCatalogoSubastasQueryHandler handler,
            [FromQuery] string estado = null,
            [FromQuery] int? categoriaId = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] string orden = "tiempo_restante",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 10)
        {
            var query = new GetCatalogoSubastasQuery
            {
                Estado = estado,
                CategoriaId = categoriaId,
                PrecioMin = precioMin,
                PrecioMax = precioMax,
                Orden = orden,
                Pagina = pagina,
                TamañoPagina = tamañoPagina
            };

            var response = await handler.HandleAsync(query);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelarSubasta(
            int id,
            [FromServices] CancelarSubastaCommandHandler handler)
        {
            // Extraemos el ID del usuario directamente desde el token (Igual que en UsuariosController)
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var command = new Application.UseCases.Subastas.CancelarSubasta.CancelarSubastaCommand
            {
                SubastaId = id,
                VendedorId = int.Parse(claimId)
            };

            await handler.HandleAsync(command);

            return Ok(new { mensaje = "La subasta fue cancelada exitosamente." });
        }
    }
}