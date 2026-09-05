using Application.UseCases.Wallet.Commands;
using Application.UseCases.Wallet.Queries;
using Application.UseCases.Wallet.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SubastaYa.Presentacion.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        public WalletController()
        {
        }

        private int ObtenerUsuarioIdDelToken()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(claimId))
                throw new Exception("No se pudo extraer el ID del usuario desde el token.");

            return int.Parse(claimId);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> ConsultarSaldo([FromServices] ConsultarSaldoQueryHandler handler)
        {
            var query = new ConsultarSaldoQuery { UsuarioId = ObtenerUsuarioIdDelToken() };
            var resultado = await handler.HandleAsync(query);
            return Ok(resultado);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> DepositarFondos(
            [FromBody] DepositarFondosCommand command,
            [FromServices] DepositarFondosCommandHandler handler)
        {
            command.UsuarioId = ObtenerUsuarioIdDelToken();
            var nuevoTotal = await handler.HandleAsync(command);

            return Ok(new
            {
                mensaje = "Acreditación simulada exitosa.",
                nuevoTotal = nuevoTotal
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> ObtenerHistorialBilletera([FromServices] ObtenerHistorialQueryHandler handler)
        {
            var query = new ObtenerHistorialQuery { UsuarioId = ObtenerUsuarioIdDelToken() };
            var resultado = await handler.HandleAsync(query);
            return Ok(resultado);
        }
    }
}