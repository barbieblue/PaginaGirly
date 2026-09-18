using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio.Repositorios
{
    // El Unit of Work agrupa TODOS los repositorios y expone un único método
    // para guardar cambios. Así, una operación que toca varias tablas a la vez
    // (ej. registrar una puja: actualiza 2 billeteras + inserta 1 puja) se
    // guarda de una sola vez, de forma atómica — o se guarda todo, o no se
    // guarda nada.
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