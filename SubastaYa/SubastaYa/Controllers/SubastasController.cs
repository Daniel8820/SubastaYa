using Application.Mappings;
using Application.Models;
using Application.UseCases.Subastas.Commands;
using Application.UseCases.Subastas.Handlers;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class SubastasController : ControllerBase
    {
        private readonly SubastaYaDbContext _context;

        public SubastasController(SubastaYaDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaRequest request)
        {
            if (request.PrecioBase <= 0 || request.IncrementoMinimo <= 0)
            {
                return BadRequest(new { error = "El precio base y el incremento mínimo deben ser mayores a cero." });
            }

            if (request.FechaFin.ToUniversalTime() <= DateTime.UtcNow)
            {
                return BadRequest(new { error = "La fecha de finalización debe ser futura." });
            }

            var nuevaSubasta = new Domain.Entities.Subasta
            {
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                UrlImagen = request.UrlImagen,
                PrecioBase = request.PrecioBase,
                IncrementoMinimo = request.IncrementoMinimo,
                FechaInicio = DateTime.UtcNow,
                FechaFin = request.FechaFin.ToUniversalTime(),
                Estado = "ACTIVA",
                VendedorId = request.VendedorId,
                CategoriaId = request.CategoriaId
            };

            _context.Subastas.Add(nuevaSubasta);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Subasta creada exitosamente", subastaId = nuevaSubasta.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerDetalleSubasta(int id)
        {
            var subasta = await _context.Subastas
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Comprador)
                .Include(s => s.Vendedor)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subasta == null)
            {
                return NotFound(new { error = "La subasta solicitada no existe." });
            }

            // Mapeo limpio mediante la capa de Aplicación
            var response = subasta.ToDetalleDto();

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCatalogo(
            [FromQuery] string estado = null,
            [FromQuery] int? categoriaId = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] string orden = "tiempo_restante",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 10)
        {
            var query = _context.Subastas
                .Include(s => s.Pujas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(s => s.Estado.ToUpper() == estado.ToUpper());
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(s => s.CategoriaId == categoriaId.Value);
            }

            if (precioMin.HasValue)
            {
                query = query.Where(s => (s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase) >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                query = query.Where(s => (s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase) <= precioMax.Value);
            }

            if (orden.ToLower() == "mayor_puja")
            {
                query = query.OrderByDescending(s => s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase);
            }
            else
            {
                query = query.OrderBy(s => s.FechaFin);
            }

            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);

            // Obtenemos las entidades paginadas desde la base de datos
            var subastasEntidades = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            // Mapeamos cada entidad a su DTO correspondiente usando el mapper de Aplicación
            var subastasDtos = subastasEntidades.Select(s => s.ToListItemDto()).ToList();

            return Ok(new
            {
                Paginacion = new
                {
                    TotalItems = totalItems,
                    TotalPaginas = totalPaginas,
                    PaginaActual = pagina,
                    TamañoPagina = tamañoPagina
                },
                Items = subastasDtos
            });
        }
    }
}