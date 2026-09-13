using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SubastaYa.Servicios.Abstracciones
{
    // Implementación "a mano" del patrón Mediator (sin usar la librería MediatR).
    // Cuando le llega un Request, busca en el contenedor de Inyección de
    // Dependencias el Handler que le corresponde, y lo ejecuta.
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            // 1) Averiguamos el tipo REAL del request en tiempo de ejecución
            //    (ej: CrearSubastaCommand), no el genérico IRequest<TResponse>.
            var requestType = request.GetType();

            // 2) Armamos el tipo de Handler que le corresponde a ESE request puntual:
            //    IRequestHandler<CrearSubastaCommand, ResultadoCreacion>
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // 3) Le pedimos al contenedor de DI (Program.cs) que nos dé la instancia
            //    de ese Handler concreto (ej: CrearSubastaCommandHandler).
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"No se encontró un Handler registrado para {requestType.Name}. " +
                    $"¿Falta el AddScoped en Program.cs?");
            }

            // 4) Invocamos el método Handle(request) de ese Handler, vía reflection,
            //    porque en este punto solo tenemos el tipo en tiempo de ejecución.
            var metodoHandle = handlerType.GetMethod("Handle");
            var tarea = (Task<TResponse>)metodoHandle!.Invoke(handler, new object[] { request })!;

            return await tarea;
        }
    }
}
