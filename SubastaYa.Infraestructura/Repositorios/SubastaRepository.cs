using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Repositorios
{
    public class SubastaRepository : Repository<Subasta>, ISubastaRepository
    {
        public SubastaRepository(SubastaYaDbContext context) : base(context) { }

        public async Task<Subasta?> GetByIdConCategoriaAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Categoria)
                .Include(s => s.Vendedor)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public IQueryable<Subasta> Consultar()
        {
            // OJO: esto devuelve la consulta SIN ejecutar todavía (no trae datos
            // de la base hasta que el Handler le agregue Where/Select y llame ToListAsync).
            return _context.Subastas.Include(s => s.Categoria).AsQueryable();
        }
    }
}