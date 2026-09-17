using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface ITransaccionRepository : IRepository<Transaccion_Ledger>
    {
        // Módulo 4: "Historial de Movimientos" de una billetera puntual.
        Task<List<Transaccion_Ledger>> GetByBilleteraIdAsync(int billeteraId);
    }
}