using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Commands
{
    public class CrearSubastaCommandHandler : IRequestHandler<CrearSubastaCommand, ResultadoCreacionDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CrearSubastaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultadoCreacionDto> Handle(CrearSubastaCommand request)
        {
            // --- Validaciones que pide la consigna (Módulo 2) ---
            // "La fecha de finalización debe ser posterior a la de inicio"
            if (request.FechaFin <= request.FechaInicio)
            {
                return new ResultadoCreacionDto
                {
                    Exito = false,
                    MensajeError = "La fecha de fin debe ser posterior a la de inicio."
                };
            }

            // "El incremento mínimo y el precio base deben ser valores positivos"
            if (request.PrecioBase <= 0 || request.IncrementoMinimo <= 0)
            {
                return new ResultadoCreacionDto
                {
                    Exito = false,
                    MensajeError = "El precio base y el incremento mínimo deben ser mayores a cero."
                };
            }

            var vendedorExiste = await _unitOfWork.Usuarios.ExisteAsync(request.VendedorId);
            if (!vendedorExiste)
            {
                return new ResultadoCreacionDto { Exito = false, MensajeError = "El vendedor indicado no existe." };
            }

            var categoriaExiste = await _unitOfWork.Categorias.ExisteAsync(request.CategoriaId);
            if (!categoriaExiste)
            {
                return new ResultadoCreacionDto { Exito = false, MensajeError = "La categoría indicada no existe." };
            }

            // El estado inicial depende de si ya arrancó o todavía no.
            var estadoInicial = request.FechaInicio <= DateTime.UtcNow ? "ACTIVA" : "PROGRAMADA";

            var nuevaSubasta = new Subasta
            {
                Vendedor_Id = request.VendedorId,
                Categoria_Id = request.CategoriaId,
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                Url_Imagen = request.UrlImagen,
                Precio_Base = request.PrecioBase,
                Incremento_Minimo = request.IncrementoMinimo,
                Fecha_Inicio = request.FechaInicio,
                Fecha_Fin = request.FechaFin,
                Estado = estadoInicial
            };

            await _unitOfWork.Subastas.AddAsync(nuevaSubasta);
            await _unitOfWork.SaveChangesAsync();

            return new ResultadoCreacionDto { Exito = true, SubastaId = nuevaSubasta.Id };
        }
    }
}
