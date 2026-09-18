using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface ISubastaRepository : IRepository<Subasta>
    {
        // Trae la subasta junto con su Categoría ya cargada (evita otra consulta aparte).
        Task<Subasta?> GetByIdConCategoriaAsync(int id);

        IQueryable<Subasta> Consultar();
    }
}