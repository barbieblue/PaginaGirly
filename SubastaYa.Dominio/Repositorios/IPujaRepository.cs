using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface IPujaRepository : IRepository<Puja>
    {
        Task<List<Puja>> GetBySubastaIdAsync(int subastaId);
    }
}
