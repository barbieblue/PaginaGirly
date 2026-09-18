using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SubastaYa.Servicios.Abstracciones
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var requestType = request.GetType();

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"No se encontró un Handler registrado para {requestType.Name}. " +
                    $"¿Falta el AddScoped en Program.cs?");
            }

            var metodoHandle = handlerType.GetMethod("Handle");
            var tarea = (Task<TResponse>)metodoHandle!.Invoke(handler, new object[] { request })!;

            return await tarea;
        }
    }
}
