using Microsoft.EntityFrameworkCore;
using SubastaYa.Infraestructura;

namespace SubastaYa.Servicios
{
    public class CatalogoService
    {
        private readonly SubastaYaDbContext _context;

        public CatalogoService(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<object> ObtenerSubastasAsync(string estado = null, string categoria = null)
        {
            var query = _context.Subastas.Include(s => s.Categoria).AsQueryable();

            // Filtro por Estado
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(s => s.Estado == estado.ToUpper());
            }

            // Filtro por Categoría (ahora navegamos a la propiedad Nombre de la entidad Categoria)
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(s => s.Categoria.Nombre.Contains(categoria));
            }

            var subastas = await query
                .Select(s => new
                {
                    s.Id,
                    s.Titulo,
                    Categoria = s.Categoria.Nombre,
                    s.Fecha_Fin,
                    s.Estado,
                    s.Url_Imagen,
                    CantidadOfertas = _context.Pujas.Count(p => p.Subasta_Id == s.Id),
                    OfertaMasAlta = _context.Pujas.Where(p => p.Subasta_Id == s.Id).Max(p => (decimal?)p.Monto) ?? s.Precio_Base
                })
                .ToListAsync();

            return subastas;
        }
    }
}