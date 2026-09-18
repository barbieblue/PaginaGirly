using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Queries
{
    public class ObtenerDetalleSubastaQuery : IRequest<SubastaDetalleDto?>
    {
        public int SubastaId { get; set; }

        public int? UsuarioId { get; set; }
    }
}