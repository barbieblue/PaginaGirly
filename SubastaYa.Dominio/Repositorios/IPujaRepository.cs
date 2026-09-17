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

        // NUEVO: permite obtener todas las pujas que hizo un comprador específico.
        // Lo usamos en el Módulo 5 ("Mis Pujas") para saber en qué subastas
        // participó un usuario dado su id.
        Task<List<Puja>> GetByCompradorIdAsync(int compradorId);
    }
}