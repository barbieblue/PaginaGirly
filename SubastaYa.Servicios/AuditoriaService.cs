using Microsoft.EntityFrameworkCore;
using SubastaYa.Infraestructura;

namespace SubastaYa.Servicios
{
    public class AuditoriaService
    {
        private readonly SubastaYaDbContext _context;

        public AuditoriaService(SubastaYaDbContext context)
        {
            _context = context;
        }

        // Trae todo el historial, más reciente primero.
        public async Task<List<object>> ObtenerTodoAsync()
        {
            return await _context.AuditoriaLogs
                .OrderByDescending(a => a.Fecha)
                .Select(a => new
                {
                    a.Id,
                    a.Entidad,
                    a.Entidad_Id,
                    a.Accion,
                    a.Usuario_Id,
                    a.Detalle_Json,
                    a.Fecha
                })
                .ToListAsync<object>();
        }

        // Filtra por una entidad puntual (ej. todos los eventos de la Subasta id 4).
        public async Task<List<object>> ObtenerPorEntidadAsync(string entidad, int entidadId)
        {
            return await _context.AuditoriaLogs
                .Where(a => a.Entidad == entidad.ToUpper() && a.Entidad_Id == entidadId)
                .OrderByDescending(a => a.Fecha)
                .Select(a => new
                {
                    a.Id,
                    a.Entidad,
                    a.Entidad_Id,
                    a.Accion,
                    a.Usuario_Id,
                    a.Detalle_Json,
                    a.Fecha
                })
                .ToListAsync<object>();
        }
    }
}