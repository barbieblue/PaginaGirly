using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Queries
{
    // SaldoDto? es nullable porque el usuario podría no tener billetera;
    // el Controller decide qué HTTP status usar.
    public class ObtenerSaldoQuery : IRequest<SaldoDto?>
    {
        public int UsuarioId { get; set; }
    }
}