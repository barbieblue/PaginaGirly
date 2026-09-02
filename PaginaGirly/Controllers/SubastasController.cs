using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios;

namespace PaginaGirly.Controllers // Asegúrate de que el namespace sea el de tu proyecto web
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly PujaService _pujaService;
        private readonly CatalogoService _catalogoService;

        // Inyectamos ambos servicios en el constructor
        public SubastasController(PujaService pujaService, CatalogoService catalogoService)
        {
            _pujaService = pujaService;
            _catalogoService = catalogoService;
        }

        // --- ENDPOINTS GET (CATÁLOGO) ---

        // GET: /api/subastas
        // Ejemplo de uso en Swagger o Postman: /api/subastas?estado=ACTIVA&categoria=Tecnología
        [HttpGet]
        public async Task<IActionResult> GetSubastas([FromQuery] string estado, [FromQuery] string categoria)
        {
            var resultado = await _catalogoService.ObtenerSubastasAsync(estado, categoria);
            return Ok(resultado);
        }


        // --- ENDPOINTS POST (PUJAS) ---

        public class NuevaPujaDto
        {
            public int UsuarioId { get; set; }
            public decimal Monto { get; set; }
        }

        // POST /api/subastas/{id}/bids
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> RegistrarPuja(int id, [FromBody] NuevaPujaDto dto)
        {
            var resultado = await _pujaService.RegistrarPujaAsync(id, dto.UsuarioId, dto.Monto);

            if (!resultado.Exito)
            {
                return StatusCode(resultado.StatusCode, new { error = resultado.MensajeError });
            }

            return Ok(new { mensaje = "Puja registrada exitosamente" });
        }
    }
}