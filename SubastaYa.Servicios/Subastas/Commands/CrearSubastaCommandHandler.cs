using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var estadoInicial = request.FechaInicio <= DateTime.UtcNow ? "ACTIVA" : "PROGRAMADA";

            var nuevaSubasta = new Subasta
            {
                Vendedor_Id = request.VendedorId,
                Categoria_Id = request.CategoriaId,
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                Url_Imagen = string.IsNullOrEmpty(request.UrlImagen)
                    ? "https://via.placeholder.com/300"
                    : request.UrlImagen,
                Precio_Base = request.PrecioBase,
                Incremento_Minimo = request.IncrementoMinimo,
                Fecha_Inicio = request.FechaInicio,
                Fecha_Fin = request.FechaFin,
                Estado = estadoInicial
            };

            try
            {
                // Acá es donde el Dominio aplica SUS PROPIAS reglas de negocio,
                // no el Handler. Si algo está mal, tira la excepción.
                nuevaSubasta.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return new ResultadoCreacionDto { Exito = false, MensajeError = ex.Message };
            }

            await _unitOfWork.Subastas.AddAsync(nuevaSubasta);
            await _unitOfWork.SaveChangesAsync();

            return new ResultadoCreacionDto { Exito = true, SubastaId = nuevaSubasta.Id };
        }
    }
}