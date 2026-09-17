using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios;

namespace PaginaGirly.Controllers
{
    [Route("api/auditoria")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaService _auditoriaService;

        public AuditoriaController(AuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        // GET /api/auditoria
        [HttpGet]
        public async Task<IActionResult> GetTodo()
        {
            var resultado = await _auditoriaService.ObtenerTodoAsync();
            return Ok(resultado);
        }

        // GET /api/auditoria/SUBASTA/4
        [HttpGet("{entidad}/{entidadId}")]
        public async Task<IActionResult> GetPorEntidad(string entidad, int entidadId)
        {
            var resultado = await _auditoriaService.ObtenerPorEntidadAsync(entidad, entidadId);
            return Ok(resultado);
        }
    }
}