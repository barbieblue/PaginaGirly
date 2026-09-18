using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface IBilleteraRepository : IRepository<Billetera>
    {
        Task<Billetera?> GetByUsuarioIdAsync(int usuarioId);
    }
}