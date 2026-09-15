using Microsoft.EntityFrameworkCore;   // para .ToListAsync()
using SubastaYa.Dominio;               // para Transaccion_Ledger
using SubastaYa.Dominio.Repositorios;  // para ITransaccionRepository

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

// Ver errores