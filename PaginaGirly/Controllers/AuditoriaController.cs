using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios.Abstracciones;
using SubastaYa.Servicios.Auditoria.Queries;

namespace PaginaGirly.Controllers
{
    [Route("api/auditoria")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditoriaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/auditoria
        [HttpGet]
        public async Task<IActionResult> GetTodo()
        {
            var resultado = await _mediator.Send(new ListarAuditoriaQuery());
            return Ok(resultado);
        }

        // GET /api/auditoria/SUBASTA/4
        [HttpGet("{entidad}/{entidadId}")]
        public async Task<IActionResult> GetPorEntidad(string entidad, int entidadId)
        {
            var resultado = await _mediator.Send(new ListarAuditoriaQuery { Entidad = entidad, EntidadId = entidadId });
            return Ok(resultado);
        }
    }
}