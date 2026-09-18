using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Auditoria.Queries
{
    public class ListarAuditoriaQueryHandler : IRequestHandler<ListarAuditoriaQuery, List<AuditoriaLogDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarAuditoriaQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AuditoriaLogDto>> Handle(ListarAuditoriaQuery request)
        {
            var logs = await _unitOfWork.AuditoriaLogs.GetAllAsync();

            IEnumerable<Dominio.Auditoria_Log> query = logs;

            if (!string.IsNullOrWhiteSpace(request.Entidad) && request.EntidadId.HasValue)
            {
                query = query.Where(a =>
                    a.Entidad == request.Entidad.ToUpper() &&
                    a.Entidad_Id == request.EntidadId.Value);
            }

            return query
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AuditoriaLogDto
                {
                    Id = a.Id,
                    Entidad = a.Entidad,
                    Entidad_Id = a.Entidad_Id,
                    Accion = a.Accion,
                    Usuario_Id = a.Usuario_Id,
                    Detalle_Json = a.Detalle_Json,
                    Fecha = a.Fecha
                })
                .ToList();
        }
    }
}