using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Queries
{
    public class ListarTransaccionesQuery : IRequest<List<TransaccionDto>?>
    {
        public int UsuarioId { get; set; }
    }
}