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

        // NUEVO: quién está mirando la subasta ahora mismo. Nullable porque
        // el usuario puede no haber elegido todavía con qué cuenta actuar
        // (ver selectUsuario en detalle.js) — en ese caso, EsLider siempre da false.
        public int? UsuarioId { get; set; }
    }
}