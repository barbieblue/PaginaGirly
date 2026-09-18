using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Repositorios
{

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SubastaYaDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(SubastaYaDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task AddAsync(T entidad) => await _dbSet.AddAsync(entidad);

        public void Update(T entidad) => _dbSet.Update(entidad);

        public void Remove(T entidad) => _dbSet.Remove(entidad);
    }
}