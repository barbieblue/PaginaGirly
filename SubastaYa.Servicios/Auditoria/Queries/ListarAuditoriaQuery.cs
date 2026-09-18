using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Auditoria.Queries
{
    public class ListarAuditoriaQuery : IRequest<List<AuditoriaLogDto>>
    {
        public string? Entidad { get; set; }
        public int? EntidadId { get; set; }
    }
}
