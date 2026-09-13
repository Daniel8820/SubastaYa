using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Wallet.DepositarFondos;
using SubastaYa.Application.UseCases.Wallet.ConsultarSaldo;
using SubastaYa.Application.UseCases.Wallet.ObtenerHistorial;
using SubastaYa.Application.Interfaces.Services;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        public WalletController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> ConsultarSaldo([FromServices] ConsultarSaldoQueryHandler handler)
        {
            var query = new ConsultarSaldoQuery { UsuarioId = _currentUser.UsuarioId };
            var resultado = await handler.HandleAsync(query);
            return Ok(resultado);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> DepositarFondos(
            [FromBody] DepositarFondosCommand command,
            [FromServices] DepositarFondosCommandHandler handler)
        {
            command.UsuarioId = _currentUser.UsuarioId;
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
            var query = new ObtenerHistorialQuery { UsuarioId = _currentUser.UsuarioId };
            var resultado = await handler.HandleAsync(query);
            return Ok(resultado);
        }
    }
}