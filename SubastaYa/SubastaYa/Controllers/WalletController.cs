using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Application.Models;

namespace SubastaYa.Presentacion.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize] // Protegemos toda la billetera
    public class WalletController : ControllerBase
    {
        private readonly SubastaYaDbContext _context;

        public WalletController(SubastaYaDbContext context)
        {
            _context = context;
        }

        // Método auxiliar para leer el ID del usuario desde el Token
        // Método auxiliar para leer el ID del usuario desde el Token
        private int ObtenerUsuarioIdDelToken()
        {
            // Buscamos el ID en el mapeo de Microsoft, y si no está, buscamos el "sub" original
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(claimId))
            {
                throw new Exception("No se pudo extraer el ID del usuario desde el token.");
            }

            return int.Parse(claimId);
        }

        // GET: /api/wallet/balance
        [HttpGet("balance")]
        public async Task<IActionResult> ConsultarSaldo()
        {
            int usuarioId = ObtenerUsuarioIdDelToken();

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                return NotFound(new { error = "No se encontró una billetera asociada a este usuario." });
            }

            var saldoDisponible = billetera.SaldoTotal - billetera.SaldoRetenido;

            return Ok(new
            {
                total = billetera.SaldoTotal,
                retenido = billetera.SaldoRetenido,
                disponible = saldoDisponible
            });
        }

        // POST: /api/wallet/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> DepositarFondos([FromBody] DepositoRequest request)
        {
            if (request.Monto <= 0)
            {
                return BadRequest(new { error = "El monto a depositar debe ser mayor a cero." });
            }

            int usuarioId = ObtenerUsuarioIdDelToken();

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                return NotFound(new { error = "No se encontró una billetera asociada a este usuario." });
            }

            // 1. Acreditación sumando al saldo total
            billetera.SaldoTotal += request.Monto;

            // 2. NUEVO: Registramos el movimiento en el Ledger para el historial
            var movimiento = new Domain.Entities.TransaccionLedger // (Ajustá el namespace si es necesario)
            {
                BilleteraId = billetera.Id,
                Tipo = "DEPOSITO",
                Monto = request.Monto,
                Fecha = DateTime.Now
                // SubastaId queda en null porque es un depósito, no una puja
            };

            _context.TransaccionesLedger.Add(movimiento);

            // Guardamos ambos cambios (el saldo y el historial) en una sola transacción
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Acreditación simulada exitosa.",
                nuevoTotal = billetera.SaldoTotal
            });
        }

        // GET: api/wallet/history
        [HttpGet("history")]
        public async Task<IActionResult> ObtenerHistorialBilletera()
        {
            int usuarioId = ObtenerUsuarioIdDelToken();

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
            {
                return NotFound(new { error = "Billetera no encontrada." });
            }

            // Consultamos el registro inmutable de transacciones
            var movimientos = await _context.TransaccionesLedger
                .Where(t => t.BilleteraId == billetera.Id)
                .OrderByDescending(t => t.Fecha)
                .Select(t => new
                {
                    id = t.Id,
                    tipo = t.Tipo, // Ej: DEPOSITO, RETENCION, LIBERACION
                    monto = t.Monto,
                    fecha = t.Fecha,
                    subastaId = t.SubastaId
                })
                .ToListAsync();

            return Ok(movimientos);
        }
    }
}