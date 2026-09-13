using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Queries
{
    // Acá vive la lógica real (ver apuntes: "Handler: donde está la lógica").
    public class ListarSubastasQueryHandler : IRequestHandler<ListarSubastasQuery, List<SubastaResumenDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarSubastasQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SubastaResumenDto>> Handle(ListarSubastasQuery request)
        {
            // Consultar() ya viene con la Categoría incluida (Include),
            // acá solo agregamos los filtros que pidió esta Query puntual.
            var query = _unitOfWork.Subastas.Consultar();

            if (!string.IsNullOrWhiteSpace(request.Estado))
                query = query.Where(s => s.Estado == request.Estado.ToUpper());

            if (!string.IsNullOrWhiteSpace(request.Categoria))
                query = query.Where(s => s.Categoria.Nombre.Contains(request.Categoria));

            var subastas = await query.ToListAsync();

            var resultado = new List<SubastaResumenDto>();

            foreach (var subasta in subastas)
            {
                // NOTA: esto dispara una consulta extra por cada subasta del
                // listado (N+1), porque IPujaRepository solo expone
                // GetBySubastaIdAsync(id) y no un método "en lote" para
                // varias subastas a la vez. Es aceptable para el volumen de
                // datos del TP; si el catálogo creciera mucho, convendría
                // sumar un método al repositorio que traiga esto de una sola
                // consulta agrupada.
                var pujas = await _unitOfWork.Pujas.GetBySubastaIdAsync(subasta.Id);

                resultado.Add(new SubastaResumenDto
                {
                    Id = subasta.Id,
                    Titulo = subasta.Titulo,
                    Categoria = subasta.Categoria.Nombre,
                    UrlImagen = subasta.Url_Imagen,
                    Estado = subasta.Estado,
                    FechaFin = subasta.Fecha_Fin,
                    CantidadOfertas = pujas.Count,
                    OfertaMasAlta = pujas.Count > 0 ? pujas.Max(p => p.Monto) : subasta.Precio_Base
                });
            }

            return resultado;
        }
    }
}
