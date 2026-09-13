using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Subastas.Queries
{
    // Lo que necesita el Módulo 1 (Catálogo) para pintar cada "card" de subasta:
    // imagen, título, categoría, oferta más alta, cantidad de ofertas y el dato
    // para armar el contador regresivo (Fecha_Fin).
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
    }
}
