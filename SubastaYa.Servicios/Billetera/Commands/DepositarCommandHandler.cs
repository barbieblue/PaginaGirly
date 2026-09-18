using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Commands
{
    public class DepositarCommandHandler : IRequestHandler<DepositarCommand, DepositarResultadoDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepositarCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DepositarResultadoDto> Handle(DepositarCommand request)
        {
            if (request.Monto <= 0)
            {
                return new DepositarResultadoDto
                {
                    Exito = false,
                    MensajeError = "El monto a depositar debe ser mayor a cero."
                };
            }

            var billetera = await _unitOfWork.Billeteras.GetByUsuarioIdAsync(request.UsuarioId);
            if (billetera == null)
            {
                return new DepositarResultadoDto
                {
                    Exito = false,
                    MensajeError = "No se encontró una billetera para ese usuario."
                };
            }

            billetera.Saldo_Total += request.Monto;

            // Módulo 4: "Historial de Movimientos: tabla con el detalle de ingresos..."
            await _unitOfWork.Transacciones.AddAsync(new Transaccion_Ledger
            {
                Billetera_Id = billetera.Id,
                Tipo = "DEPOSITO",
                Monto = request.Monto,
                Fecha = DateTime.UtcNow
            });

            // Auditoría: la consigna pide registrar acreditaciones manuales de saldo.
            // El Ledger registra el movimiento contable; este log registra el evento
            // de negocio de forma inmutable y trazable.
            await _unitOfWork.AuditoriaLogs.AddAsync(new Auditoria_Log
            {
                Entidad = "BILLETERA",
                Entidad_Id = billetera.Id,
                Accion = "DEPOSITO_MANUAL",
                Usuario_Id = request.UsuarioId,
                Detalle_Json = $"{{\"monto\": {request.Monto}}}",
                Fecha = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();

            return new DepositarResultadoDto
            {
                Exito = true,
                SaldoTotalActualizado = billetera.Saldo_Total
            };
        }
    }
}