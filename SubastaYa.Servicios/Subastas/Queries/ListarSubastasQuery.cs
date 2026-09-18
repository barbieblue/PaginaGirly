using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Queries
{
    public class ListarSubastasQuery : IRequest<List<SubastaResumenDto>>
    {
        
        public string? Estado { get; set; }
        public string? Categoria { get; set; }

        
        public int? VendedorId { get; set; }


        public int? CompradorId { get; set; }
    }
}