using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    // Además de lo genérico, cada repositorio puede sumar consultas propias
    // que solo tienen sentido para esa entidad puntual.
    public interface ISubastaRepository : IRepository<Subasta>
    {
        // Trae la subasta junto con su Categoría ya cargada (evita otra consulta aparte).
        Task<Subasta?> GetByIdConCategoriaAsync(int id);

        // Devuelve un IQueryable para que el Handler arme el filtro que necesite
        // (por estado, por categoría, etc.) sin que el repositorio tenga que
        // prever de antemano cada combinación posible de filtros.
        IQueryable<Subasta> Consultar();
    }
}