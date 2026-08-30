using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Persistence;

namespace SubastaYa.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize] // Todo este controlador requiere estar logueado
    public class UsuariosController : ControllerBase
    {
        private readonly SubastaYaDbContext _context;

        public UsuariosController(SubastaYaDbContext context)
        {
            _context = context;
        }

        // Método auxiliar para leer el ID del usuario desde el Token (igual que en Wallet)
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

        // GET: api/v1/users/me/activities
        [HttpGet("me/activities")]
        public async Task<IActionResult> MisActividades()
        {
            int usuarioId = ObtenerUsuarioIdDelToken();

            // 1. Buscamos las subastas donde el usuario es el VENDEDOR
            var misPublicaciones = await _context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.VendedorId == usuarioId)
                .Select(s => new
                {
                    id = s.Id,
                    titulo = s.Titulo,
                    estado = s.Estado,
                    // Si hay pujas, la recaudación es la máxima; si no, 0
                    recaudacion = s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : 0,
                    adjudicada = s.Estado == "FINALIZADA" && s.Pujas.Any()
                })
                .ToListAsync();

            // 2. Buscamos las subastas donde el usuario ofertó (COMPRADOR)
            var misPujas = await _context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.Pujas.Any(p => p.CompradorId == usuarioId))
                .Select(s => new
                {
                    id = s.Id,
                    titulo = s.Titulo,
                    estado = s.Estado,
                    miOfertaMaxima = s.Pujas.Where(p => p.CompradorId == usuarioId).Max(p => p.Monto),
                    ofertaGanadoraActual = s.Pujas.Max(p => p.Monto),
                    // Evaluamos si la subasta terminó y si la puja más alta es de este usuario
                    soyGanador = s.Estado == "FINALIZADA" &&
                                 s.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault().CompradorId == usuarioId
                })
                .ToListAsync();

            // Retornamos ambos listados en un solo objeto JSON
            return Ok(new
            {
                misPublicaciones,
                misComprasYPujas = misPujas
            });
        }
    }
}