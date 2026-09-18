using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Subastas.Queries
{
    public class SubastaResumenDto
    {
        // Ya existían: datos que muestra el catálogo en cada card.
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? UrlImagen { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaFin { get; set; }
        public decimal OfertaMasAlta { get; set; }
        public int CantidadOfertas { get; set; }

        // NUEVO: indica si el usuario que consultó "Mis Pujas" es el postor
        // líder actual en esta subasta. Se usa en el Módulo 5 para mostrar
        // una etiqueta "Liderando" o "Superado" en cada card.
        public bool EsLider { get; set; }
    }
}