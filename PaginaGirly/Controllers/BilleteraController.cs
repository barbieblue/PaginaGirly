using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios;

namespace PaginaGirly.Controllers
{
    [Route("api/billetera")]
    [ApiController]
    public class BilleteraController : ControllerBase
    {
        private readonly BilleteraService _billeteraService;

        public BilleteraController(BilleteraService billeteraService)
        {
            _billeteraService = billeteraService;
        }

        // GET /api/wallet/balance?usuarioId=3
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] int usuarioId)
        {
            var saldo = await _billeteraService.ObtenerSaldoAsync(usuarioId);

            if (saldo == null)
            {
                return NotFound(new { error = "No se encontró una billetera para ese usuario." });
            }

            return Ok(saldo);
        }

        // Cuerpo esperado para el depósito.
        public class DepositoDto
        {
            public int UsuarioId { get; set; }
            public decimal Monto { get; set; }
        }

        // POST /api/wallet/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositoDto dto)
        {
            var exito = await _billeteraService.DepositarAsync(dto.UsuarioId, dto.Monto);

            if (!exito)
            {
                return BadRequest(new { error = "No se pudo acreditar el depósito. Verificá el usuario y que el monto sea positivo." });
            }

            return Ok(new { mensaje = "Depósito acreditado exitosamente." });
        }

        // GET /api/wallet/4/transacciones
        [HttpGet("{usuarioId}/transacciones")]
        public async Task<IActionResult> GetTransacciones(int usuarioId)
        {
            var transacciones = await _billeteraService.ObtenerTransaccionesAsync(usuarioId);

            if (transacciones == null)
            {
                return NotFound(new { error = "No se encontró una billetera para ese usuario." });
            }

            return Ok(transacciones);
        }
    }
}