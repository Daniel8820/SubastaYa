using Application.UseCases.Subastas.Commands;
using Application.UseCases.Subastas.Handlers;
using Application.UseCases.Subastas.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SubastaYa.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class SubastasController : ControllerBase
    {
        // ¡ADIÓS DbContext! Controlador 100% limpio.
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
            command.SubastaId = id;
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
            int subastaId = await handler.HandleAsync(command);

            // REST Nivel 2: Devolvemos 201 Created y le decimos al cliente 
            // cómo llamar al GET ObtenerDetalleSubasta pasándole el nuevo ID
            return CreatedAtAction(
                nameof(ObtenerDetalleSubasta), // El nombre del método GET
                new { id = subastaId },        // El parámetro de ruta que necesita el GET
                new { mensaje = "Subasta creada exitosamente", subastaId = subastaId } // El cuerpo de la respuesta
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
    }
}