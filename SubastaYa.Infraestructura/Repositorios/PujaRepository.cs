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
    public class PujaRepository : Repository<Puja>, IPujaRepository
    {
        public PujaRepository(SubastaYaDbContext context) : base(context) { }

        public async Task<List<Puja>> GetBySubastaIdAsync(int subastaId)
        {
            return await _context.Pujas.Where(p => p.Subasta_Id == subastaId).ToListAsync();
        }

        public async Task<List<Puja>> GetByCompradorIdAsync(int compradorId)
        {
            return await _context.Pujas.Where(p => p.Comprador_Id == compradorId).ToListAsync();
        }
    }
}