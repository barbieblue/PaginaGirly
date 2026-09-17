using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Subastas.Queries
{
    public class SubastaDetalleDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public decimal IncrementoMinimo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? UrlImagen { get; set; }
        public decimal OfertaMasAlta { get; set; }
        public int CantidadOfertas { get; set; }
        public decimal ProximaOfertaSugerida { get; set; }
        public List<PujaDetalleDto> Pujas { get; set; } = new();
    }

    public class PujaDetalleDto
    {
        public string Usuario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPuja { get; set; }
    }
}