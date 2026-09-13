using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Abstracciones
{
    // Interfaz "marcadora": no tiene métodos, solo sirve para decir
    // "esta clase es un Command o una Query, y al procesarla se espera
    // una respuesta de tipo TResponse".
    // Ejemplos: CrearSubastaCommand : IRequest<ResultadoCreacion>
    //           ListarSubastasQuery : IRequest<List<SubastaDto>>
    public interface IRequest<TResponse>
    {
    }
}
