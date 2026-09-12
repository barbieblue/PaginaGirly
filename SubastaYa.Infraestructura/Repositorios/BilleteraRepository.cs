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
    public class BilleteraRepository : Repository<Billetera>, IBilleteraRepository
    {
        public BilleteraRepository(SubastaYaDbContext context) : base(context) { }

        public async Task<Billetera?> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Billeteras.FirstOrDefaultAsync(b => b.Usuario_Id == usuarioId);
        }
    }
}