using Microsoft.AspNetCore.Mvc;
using SubastaYa.Servicios;

namespace PaginaGirly.Controllers
{
    [Route("api/categorias")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly CatalogoService _catalogoService;

        public CategoriasController(CatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        // GET /api/categorias
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _catalogoService.ObtenerCategoriasAsync();
            return Ok(categorias);
        }
    }
}