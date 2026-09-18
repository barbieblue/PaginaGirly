using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios.Abstracciones;
using SubastaYa.Servicios.Subastas.Queries;
using SubastaYa.Servicios.Subastas.Commands;

namespace PaginaGirly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubastasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubastas(
            [FromQuery] string? estado,
            [FromQuery] string? categoria,
            [FromQuery] int? vendedorId,
            [FromQuery] int? compradorId)
        {
            var resultado = await _mediator.Send(new ListarSubastasQuery
            {
                Estado = estado,
                Categoria = categoria,
                VendedorId = vendedorId,
                CompradorId = compradorId
            });
            return Ok(resultado);
        }

        // GET /api/subastas/{id}?usuarioId=2
        // usuarioId opcional, para que el Handler calcule EsLider
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id, [FromQuery] int? usuarioId)
        {
            var detalle = await _mediator.Send(new ObtenerDetalleSubastaQuery { SubastaId = id, UsuarioId = usuarioId });

            if (detalle == null)
            {
                return NotFound(new { mensaje = "Subasta no encontrada." });
            }

            return Ok(detalle);
        }

        public class NuevaPujaDto
        {
            public int UsuarioId { get; set; }
            public decimal Monto { get; set; }
        }

        // POST /api/subastas/{id}/bids
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> RegistrarPuja(int id, [FromBody] NuevaPujaDto dto)
        {
            var comando = new RegistrarPujaCommand
            {
                SubastaId = id,
                UsuarioId = dto.UsuarioId,
                Monto = dto.Monto
            };

            var resultado = await _mediator.Send(comando);

            if (!resultado.Exito)
            {
                return StatusCode(resultado.StatusCode, new { error = resultado.MensajeError });
            }

            return Ok(new { mensaje = "Puja registrada exitosamente" });
        }

        // POST /api/subastas
        [HttpPost]
        public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaCommand comando)
        {
            var resultado = await _mediator.Send(comando);

            if (!resultado.Exito)
            {
                return BadRequest(new { error = resultado.MensajeError });
            }

            return CreatedAtAction(nameof(GetDetalle), new { id = resultado.SubastaId }, new { id = resultado.SubastaId });
        }
    }
}