using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Subastas.Queries
{
    public class SubastaResumenDto
    {
       
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? UrlImagen { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaFin { get; set; }
        public decimal OfertaMasAlta { get; set; }
        public int CantidadOfertas { get; set; }

        public bool EsLider { get; set; }
    }
}