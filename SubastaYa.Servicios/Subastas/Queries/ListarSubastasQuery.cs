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
        // Ya existían: filtros del catálogo (Módulo 1).
        public string? Estado { get; set; }
        public string? Categoria { get; set; }

        // NUEVO: filtro por vendedor, para "Mis Publicaciones" (Módulo 5).
        // Cuando se manda este id, el handler devuelve solo las subastas
        // creadas por ese usuario.
        public int? VendedorId { get; set; }

        // NUEVO: filtro por comprador, para "Mis Pujas" (Módulo 5).
        // Cuando se manda este id, el handler devuelve solo las subastas
        // en las que ese usuario realizó al menos una puja.
        public int? CompradorId { get; set; }
    }
}