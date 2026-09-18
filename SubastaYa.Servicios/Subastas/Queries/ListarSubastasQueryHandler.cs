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

            // Filtro ya existente: por estado (ACTIVA, FINALIZADA, etc.)
            if (!string.IsNullOrWhiteSpace(request.Estado))
                query = query.Where(s => s.Estado == request.Estado.ToUpper());

            // Filtro ya existente: por nombre de categoría.
            if (!string.IsNullOrWhiteSpace(request.Categoria))
                query = query.Where(s => s.Categoria.Nombre.Contains(request.Categoria));

            // NUEVO: filtro por vendedor para "Mis Publicaciones" (Módulo 5).
            // Si se manda VendedorId, devuelve solo las subastas creadas por ese usuario.
            if (request.VendedorId.HasValue)
                query = query.Where(s => s.Vendedor_Id == request.VendedorId.Value);

            var subastas = await query.ToListAsync();

            // NUEVO: filtro por comprador para "Mis Pujas" (Módulo 5).
            // Obtenemos los ids de subastas donde el usuario pujó al menos una vez,
            // y descartamos las subastas que no estén en esa lista.
            if (request.CompradorId.HasValue)
            {
                var pujasPorComprador = await _unitOfWork.Pujas.GetByCompradorIdAsync(request.CompradorId.Value);
                var subastaIdsPujadas = pujasPorComprador.Select(p => p.Subasta_Id).Distinct().ToList();
                subastas = subastas.Where(s => subastaIdsPujadas.Contains(s.Id)).ToList();
            }

            var resultado = new List<SubastaResumenDto>();

            foreach (var subasta in subastas)
            {
                // NOTA: esto dispara una consulta extra por cada subasta (N+1).
                // Es aceptable para el volumen de datos del TP.
                var pujas = await _unitOfWork.Pujas.GetBySubastaIdAsync(subasta.Id);
                var pujaMaxima = pujas.Count > 0 ? pujas.Max(p => p.Monto) : subasta.Precio_Base;

                // NUEVO: determina si el usuario de "Mis Pujas" es el líder actual
                // en esta subasta. Se usa en el frontend para mostrar "Liderando" o "Superado".
                // Solo se calcula cuando se filtró por comprador; en el catálogo general es false.
                bool esLider = request.CompradorId.HasValue &&
                               pujas.Any() &&
                               pujas.OrderByDescending(p => p.Monto).First().Comprador_Id == request.CompradorId.Value;

                resultado.Add(new SubastaResumenDto
                {
                    Id = subasta.Id,
                    Titulo = subasta.Titulo,
                    Categoria = subasta.Categoria.Nombre,
                    UrlImagen = subasta.Url_Imagen,
                    Estado = subasta.Estado,
                    FechaFin = subasta.Fecha_Fin,
                    CantidadOfertas = pujas.Count,
                    OfertaMasAlta = pujaMaxima,
                    // NUEVO: incluido en el DTO para que el frontend pueda
                    // mostrar el estado del usuario en cada subasta.
                    EsLider = esLider
                });
            }

            return resultado;
        }
    }
}