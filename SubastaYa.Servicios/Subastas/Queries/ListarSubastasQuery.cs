using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Queries
{
    // La Query solo transporta los datos del filtro. No tiene lógica adentro
    // (ver apuntes: "Query: van a decir qué datos necesito para pedir datos").
    public class ListarSubastasQuery : IRequest<List<SubastaResumenDto>>
    {
        public string? Estado { get; set; }
        public string? Categoria { get; set; }
    }
}
