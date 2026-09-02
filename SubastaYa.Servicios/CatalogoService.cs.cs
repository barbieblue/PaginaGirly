using Microsoft.EntityFrameworkCore;
using SubastaYa.Infraestructura; // Tu DbContext

namespace SubastaYa.Servicios
{
    public class CatalogoService
    {
        private readonly SubastaYaDbContext _context;

        public CatalogoService(SubastaYaDbContext context)
        {
            _context = context;
        }

        // Método para obtener el listado filtrado
        public async Task<object> ObtenerSubastasAsync(string estado = null, string categoria = null)
        {
            // Empezamos armando la consulta básica
            var query = _context.Subastas.AsQueryable();

            // 1. Filtro por Estado (Ej: "ACTIVA", "PROXIMA", "FINALIZADA")
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(s => s.Estado == estado.ToUpper());
            }

            // 2. Filtro por Categoría
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(s => s.Categoria.Contains(categoria));
            }

            // 3. Seleccionamos solo los datos necesarios para las Cards del Frontend (como pide el PDF)
            // Calculamos "al vuelo" la cantidad de ofertas y la puja más alta
            var subastas = await query
                .Select(s => new
                {
                    s.Id,
                    s.Titulo,
                    s.Categoria,
                    s.FechaFin,
                    s.Estado,
                    CantidadOfertas = _context.Pujas.Count(p => p.SubastaId == s.Id),
                    OfertaMasAlta = _context.Pujas.Where(p => p.SubastaId == s.Id).Max(p => (decimal?)p.Monto) ?? s.PrecioBase
                })
                .ToListAsync();

            return subastas;
        }
    }
}