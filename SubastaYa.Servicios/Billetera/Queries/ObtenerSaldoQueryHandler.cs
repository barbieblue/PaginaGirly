using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Queries
{
    public class ObtenerSaldoQueryHandler : IRequestHandler<ObtenerSaldoQuery, SaldoDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtenerSaldoQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SaldoDto?> Handle(ObtenerSaldoQuery request)
        {
            var billetera = await _unitOfWork.Billeteras.GetByUsuarioIdAsync(request.UsuarioId);

            if (billetera == null)
            {
                return null; // el Controller decide si esto es 404
            }

            return new SaldoDto
            {
                SaldoTotal = billetera.Saldo_Total,
                SaldoRetenido = billetera.Saldo_Retenido,
                SaldoDisponible = billetera.Saldo_Disponible // propiedad calculada del Dominio
            };
        }
    }
}
