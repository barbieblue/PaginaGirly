using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Queries
{
    public class ObtenerDetalleSubastaQueryHandler : IRequestHandler<ObtenerDetalleSubastaQuery, SubastaDetalleDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtenerDetalleSubastaQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SubastaDetalleDto?> Handle(ObtenerDetalleSubastaQuery request)
        {
            var subasta = await _unitOfWork.Subastas.GetByIdConCategoriaAsync(request.SubastaId);

            if (subasta == null)
            {
                return null;
            }

            var pujas = await _unitOfWork.Pujas.GetBySubastaIdAsync(subasta.Id);
            var ofertaMasAlta = pujas.Count > 0 ? pujas.Max(p => p.Monto) : subasta.Precio_Base;

            return new SubastaDetalleDto
            {
                Id = subasta.Id,
                Titulo = subasta.Titulo,
                Descripcion = subasta.Descripcion,
                Categoria = subasta.Categoria.Nombre,
                Vendedor = subasta.Vendedor.Nombre,
                PrecioBase = subasta.Precio_Base,
                IncrementoMinimo = subasta.Incremento_Minimo,
                FechaInicio = subasta.Fecha_Inicio,
                FechaFin = subasta.Fecha_Fin,
                Estado = subasta.Estado,
                UrlImagen = subasta.Url_Imagen,
                OfertaMasAlta = ofertaMasAlta,
                CantidadOfertas = pujas.Count,
                ProximaOfertaSugerida = ofertaMasAlta + subasta.Incremento_Minimo
            };
        }
    }
}
