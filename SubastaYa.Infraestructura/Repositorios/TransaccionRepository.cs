using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Repositorios
{
    public class TransaccionRepository : Repository<Transaccion_Ledger>, ITransaccionRepository
    {
        public TransaccionRepository(SubastaYaDbContext context) : base(context) { }

        public async Task<List<Transaccion_Ledger>> GetByBilleteraIdAsync(int billeteraId)
        {
            return await _dbSet
                .Where(t => t.Billetera_Id == billeteraId)
                .ToListAsync();
        }
    }
}
