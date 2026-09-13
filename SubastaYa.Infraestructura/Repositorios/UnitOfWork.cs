using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Repositorios
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SubastaYaDbContext _context;

        public ISubastaRepository Subastas { get; }
        public IUsuarioRepository Usuarios { get; }
        public ICategoriaRepository Categorias { get; }
        public IBilleteraRepository Billeteras { get; }
        public IPujaRepository Pujas { get; }
        public ITransaccionRepository Transacciones { get; }
        public IAuditoriaRepository AuditoriaLogs { get; }

        public UnitOfWork(SubastaYaDbContext context)
        {
            _context = context;
            // Todos los repositorios comparten la MISMA instancia de _context,
            // por eso cuando llamás SaveChangesAsync una sola vez, guarda
            // los cambios de todos ellos juntos, atómicamente.
            Subastas = new SubastaRepository(context);
            Usuarios = new UsuarioRepository(context);
            Categorias = new CategoriaRepository(context);
            Billeteras = new BilleteraRepository(context);
            Pujas = new PujaRepository(context);
            Transacciones = new TransaccionRepository(context);
            AuditoriaLogs = new AuditoriaRepository(context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}