using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Queries
{
    public class ListarTransaccionesQueryHandler : IRequestHandler<ListarTransaccionesQuery, List<TransaccionDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarTransaccionesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TransaccionDto>?> Handle(ListarTransaccionesQuery request)
        {
            var billetera = await _unitOfWork.Billeteras.GetByUsuarioIdAsync(request.UsuarioId);

            if (billetera == null)
            {
                return null;
            }

            var transacciones = await _unitOfWork.Transacciones.GetByBilleteraIdAsync(billetera.Id);

            return transacciones
                .OrderByDescending(t => t.Fecha)
                .Select(t => new TransaccionDto
                {
                    Id = t.Id,
                    Tipo = t.Tipo,
                    Monto = t.Monto,
                    Fecha = t.Fecha,
                    SubastaId = t.Subasta_Id
                })
                .ToList();
        }
    }
}