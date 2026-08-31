using Application.Interfaces.Services;
using Application.Models;
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
        private readonly ISubastaService _subastaService; // Agregamos el servicio

        // Inyectamos el servicio en el constructor
        public SubastasController(SubastaYaDbContext context, ISubastaService subastaService)
        {
            _context = context;
            _subastaService = subastaService;
        }

        // POST: api/v1/auctions/{id}/bids
        [Authorize]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> RegistrarPuja(int id, [FromBody] RegistroPujaRequest request)
        {
            // Aseguramos que el ID de la URL coincida con el de la petición
            request.SubastaId = id;

            try
            {
                var resultado = await _subastaService.RegistrarPujaAsync(request);

                if (resultado)
                {
                    return Ok(new { mensaje = "Puja registrada exitosamente. Saldo retenido temporalmente." });
                }

                // ACÁ ESTÁ EL CAMBIO: Si devuelve false, sabemos con seguridad que fue por el choque de concurrencia
                return Conflict(new { error = "Rechazo por concurrencia. Otro usuario acaba de pujar. Por favor, actualizá la subasta e intentá nuevamente." });
            }
            catch (Exception ex)
            {
                // Devolvemos el mensaje de la excepción (Nuestras validaciones de negocio)
                // En un entorno de producción estricto esto se suele ocultar, pero para el TP es ideal
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/v1/auctions
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaRequest request)
        {
            if (request.PrecioBase <= 0 || request.IncrementoMinimo <= 0)
            {
                return BadRequest(new { error = "El precio base y el incremento mínimo deben ser mayores a cero." });
            }

            // Convertimos la hora que mandó el usuario a UTC solo para hacer la comprobación
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

        // GET: api/v1/auctions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerDetalleSubasta(int id)
        {
            var subasta = await _context.Subastas
                .Include(s => s.Pujas) // Traemos las ofertas asociadas
                    .ThenInclude(p => p.Comprador) // Incluimos los datos del comprador que ofertó
                .Include(s => s.Vendedor) // Incluimos los datos de quien la publica
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subasta == null)
            {
                return NotFound(new { error = "La subasta solicitada no existe." });
            }

            // Mapeamos a una respuesta limpia para que no se acople directamente la entidad de base de datos
            var response = new
            {
                id = subasta.Id,
                titulo = subasta.Titulo,
                descripcion = subasta.Descripcion,
                urlImagen = subasta.UrlImagen,
                precioBase = subasta.PrecioBase,
                incrementoMinimo = subasta.IncrementoMinimo,
                fechaInicio = subasta.FechaInicio,
                fechaFin = subasta.FechaFin,
                estado = subasta.Estado,
                vendedor = subasta.Vendedor != null ? subasta.Vendedor.Nombre : "Desconocido",
                pujasTotal = subasta.Pujas.Count,
                historialPujas = subasta.Pujas
                    .OrderByDescending(p => p.Monto) // La más alta primero
                    .Select(p => new
                    {
                        monto = p.Monto,
                        fecha = p.FechaPuja,
                        comprador = p.Comprador != null ? p.Comprador.Nombre : "Anónimo"
                    })
            };

            return Ok(response);
        }

        // GET: api/v1/auctions
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
            // 1. Iniciamos la consulta base incluyendo las relaciones necesarias
            var query = _context.Subastas
                .Include(s => s.Pujas)
                .AsQueryable();

            // 2. Aplicamos los filtros dinámicos
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
                // Filtramos comparando contra la oferta más alta o el precio base si no hay pujas
                query = query.Where(s => (s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase) >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                query = query.Where(s => (s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase) <= precioMax.Value);
            }

            // 3. Aplicamos el ordenamiento
            if (orden.ToLower() == "mayor_puja")
            {
                query = query.OrderByDescending(s => s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase);
            }
            else
            {
                // Por defecto, ordenamos por las que terminan más pronto (tiempo restante)
                query = query.OrderBy(s => s.FechaFin);
            }

            // 4. Calculamos el total para la paginación antes de recortar los datos
            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);

            // 5. Aplicamos paginación y mapeamos al formato de salida (Cards de Producto)
            var subastas = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .Select(s => new
                {
                    id = s.Id,
                    titulo = s.Titulo,
                    urlImagen = s.UrlImagen,
                    estado = s.Estado,
                    ofertaMasAlta = s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase,
                    cantidadOfertas = s.Pujas.Count,
                    fechaFin = s.FechaFin
                })
                .ToListAsync();

            // 6. Retornamos la respuesta estructurada
            return Ok(new
            {
                Paginacion = new
                {
                    TotalItems = totalItems,
                    TotalPaginas = totalPaginas,
                    PaginaActual = pagina,
                    TamañoPagina = tamañoPagina
                },
                Items = subastas
            });
        }
    }
}