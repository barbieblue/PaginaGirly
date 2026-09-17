using Microsoft.EntityFrameworkCore;
using SubastaYa.Infraestructura;

namespace SubastaYa.Servicios
{
    public class UsuarioService
    {
        private readonly SubastaYaDbContext _context;

        public UsuarioService(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> ObtenerUsuariosAsync()
        {
            return await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email
                })
                .ToListAsync<object>();
        }
    }
}