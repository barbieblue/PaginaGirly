using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Abstracciones
{
    // El Controller solo conoce esta interfaz. No sabe (ni le importa)
    // qué clase de Handler procesa cada Request: se lo delega al Mediator.
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
    }
}