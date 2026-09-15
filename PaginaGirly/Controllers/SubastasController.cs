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
        // Ya no recibe PujaService ni CatalogoService: solo el Mediator.
        private readonly IMediator _mediator;

        public SubastasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- ENDPOINTS GET (CATÁLOGO) ---

        // GET /api/subastas?estado=ACTIVA&categoria=Tecnología
        [HttpGet]
        public async Task<IActionResult> GetSubastas([FromQuery] string? estado, [FromQuery] string? categoria)
        {
            var resultado = await _mediator.Send(new ListarSubastasQuery { Estado = estado, Categoria = categoria });
            return Ok(resultado);
        }

        // GET /api/subastas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var detalle = await _mediator.Send(new ObtenerDetalleSubastaQuery { SubastaId = id });

            if (detalle == null)
            {
                return NotFound(new { mensaje = "Subasta no encontrada." });
            }

            return Ok(detalle);
        }

        // --- ENDPOINTS POST (PUJAS Y CREACIÓN) ---

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

            // 201 Created es el código correcto para "se creó un recurso nuevo".
            return CreatedAtAction(nameof(GetDetalle), new { id = resultado.SubastaId }, new { id = resultado.SubastaId });
        }
    }
}