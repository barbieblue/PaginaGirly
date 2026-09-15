using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios.Abstracciones;
using SubastaYa.Servicios.Billetera.Queries;
using SubastaYa.Servicios.Billetera.Commands;

namespace PaginaGirly.Controllers
{
    [ApiController]
    [Route("api/billetera")]
    public class BilleteraController : ControllerBase
    {
        // El Controller ya no conoce ningún Service: solo arma el Command/Query
        // y se lo manda al Mediator, que sabe qué Handler lo resuelve.
        private readonly IMediator _mediator;

        public BilleteraController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/wallet/balance?usuarioId=2
        [HttpGet("balance")]
        public async Task<IActionResult> ObtenerSaldo([FromQuery] int usuarioId)
        {
            var saldo = await _mediator.Send(new ObtenerSaldoQuery { UsuarioId = usuarioId });

            if (saldo == null)
            {
                return NotFound(new { mensaje = "No se encontró una billetera para ese usuario." });
            }

            return Ok(saldo);
        }

        // GET /api/wallet/transactions?usuarioId=2
        [HttpGet("transactions")]
        public async Task<IActionResult> ObtenerTransacciones([FromQuery] int usuarioId)
        {
            var transacciones = await _mediator.Send(new ListarTransaccionesQuery { UsuarioId = usuarioId });

            if (transacciones == null)
            {
                return NotFound(new { mensaje = "No se encontró una billetera para ese usuario." });
            }

            return Ok(transacciones);
        }

        // POST /api/wallet/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Depositar([FromBody] DepositarCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (!resultado.Exito)
            {
                return BadRequest(new { mensaje = resultado.MensajeError });
            }

            return Ok(new { saldoTotal = resultado.SaldoTotalActualizado });
        }
    }
}