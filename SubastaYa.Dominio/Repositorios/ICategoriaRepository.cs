using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface ICategoriaRepository : IRepository<Categoria>
    {
        Task<bool> ExisteAsync(int id);
    }
}