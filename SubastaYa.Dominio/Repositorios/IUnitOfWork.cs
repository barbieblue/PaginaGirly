using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    public interface IUnitOfWork
    {
        ISubastaRepository Subastas { get; }
        IUsuarioRepository Usuarios { get; }
        ICategoriaRepository Categorias { get; }
        IBilleteraRepository Billeteras { get; }
        IPujaRepository Pujas { get; }
        ITransaccionRepository Transacciones { get; }
        IAuditoriaRepository AuditoriaLogs { get; }
        Task<int> SaveChangesAsync();
    }
}