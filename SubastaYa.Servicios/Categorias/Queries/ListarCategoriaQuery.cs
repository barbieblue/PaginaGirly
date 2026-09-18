using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Categorias.Queries
{
    // No necesita ningún parámetro: siempre trae todas. Sigue siendo un
    // IRequest válido aunque no tenga propiedades.
    public class ListarCategoriasQuery : IRequest<List<CategoriaDto>>
    {
    }
}
