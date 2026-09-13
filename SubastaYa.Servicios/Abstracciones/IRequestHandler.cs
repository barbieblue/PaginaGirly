using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Abstracciones
{
    // Quien sabe ejecutar un Request puntual y devolver su resultado.
    // Cada Command/Query tiene UN solo Handler (1 a 1), y ahí es donde
    // vive la lógica de negocio (ver apuntes: "Handler: donde está la
    // lógica. Reglas y evaluaciones").
    public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }
}
