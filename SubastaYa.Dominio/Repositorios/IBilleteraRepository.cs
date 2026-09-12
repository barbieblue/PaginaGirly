using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface IBilleteraRepository : IRepository<Billetera>
    {
        // La consulta más usada de todo el TP: la billetera de un usuario puntual.
        Task<Billetera?> GetByUsuarioIdAsync(int usuarioId);
    }
}