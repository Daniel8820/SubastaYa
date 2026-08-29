using Application.Models;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Presentacion.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")] // Estándar RESTful: sustantivo plural sugerido por la cátedra
    public class SubastasController : ControllerBase
    {
        private readonly SubastaYaDbContext _context;

        public SubastasController(SubastaYaDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/auctions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubastaResponse>>> GetSubastas()
        {
            var subastas = await _context.Subastas
                .Include(s => s.Pujas) // Traemos las pujas para calcular los totales
                .Select(s => new SubastaResponse
                {
                    Id = s.Id,
                    Titulo = s.Titulo,
                    Descripcion = s.Descripcion,
                    UrlImagen = s.UrlImagen,
                    PrecioBase = s.PrecioBase,
                    FechaFin = s.FechaFin,
                    Estado = s.Estado,

                    // Calculamos al vuelo la oferta más alta y la cantidad de pujas
                    CantidadOfertas = s.Pujas.Count(),
                    OfertaMasAlta = s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase
                })
                .ToListAsync();

            return Ok(subastas); // Retorna código 200 OK
        }
    }
}