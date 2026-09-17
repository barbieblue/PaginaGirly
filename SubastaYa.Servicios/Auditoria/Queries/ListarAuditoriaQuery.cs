using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Auditoria.Queries
{
    // Si Entidad/EntidadId vienen null, se trae TODO el historial (caso
    // "GetTodo" del Controller viejo). Si vienen cargados, filtra por esa
    // entidad puntual (caso "GetPorEntidad").
    public class ListarAuditoriaQuery : IRequest<List<AuditoriaLogDto>>
    {
        public string? Entidad { get; set; }
        public int? EntidadId { get; set; }
    }
}
