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
   
            var query = _unitOfWork.Subastas.Consultar();

          
            if (!string.IsNullOrWhiteSpace(request.Estado))
                query = query.Where(s => s.Estado == request.Estado.ToUpper());

       
            if (!string.IsNullOrWhiteSpace(request.Categoria))
                query = query.Where(s => s.Categoria.Nombre.Contains(request.Categoria));

      
            if (request.VendedorId.HasValue)
                query = query.Where(s => s.Vendedor_Id == request.VendedorId.Value);

            var subastas = await query.ToListAsync();

         
            if (request.CompradorId.HasValue)
            {
                var pujasPorComprador = await _unitOfWork.Pujas.GetByCompradorIdAsync(request.CompradorId.Value);
                var subastaIdsPujadas = pujasPorComprador.Select(p => p.Subasta_Id).Distinct().ToList();
                subastas = subastas.Where(s => subastaIdsPujadas.Contains(s.Id)).ToList();
            }

            var resultado = new List<SubastaResumenDto>();

            foreach (var subasta in subastas)
            {
               
                var pujas = await _unitOfWork.Pujas.GetBySubastaIdAsync(subasta.Id);
                var pujaMaxima = pujas.Count > 0 ? pujas.Max(p => p.Monto) : subasta.Precio_Base;

             
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
 
                    EsLider = esLider
                });
            }

            return resultado;
        }
    }
}