using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Commands
{
    public class RegistrarPujaCommandHandler : IRequestHandler<RegistrarPujaCommand, ResultadoPujaDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarPujaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultadoPujaDto> Handle(RegistrarPujaCommand request)
        {
            try
            {
                var subasta = await _unitOfWork.Subastas.GetByIdAsync(request.SubastaId);

                if (subasta == null || subasta.Estado != "ACTIVA" || subasta.Fecha_Fin < DateTime.UtcNow)
                {
                    return new ResultadoPujaDto
                    {
                        Exito = false,
                        MensajeError = "Subasta no válida o finalizada.",
                        StatusCode = 400
                    };
                }

                var pujasPrevias = await _unitOfWork.Pujas.GetBySubastaIdAsync(request.SubastaId);
                var pujaMaxima = pujasPrevias.OrderByDescending(p => p.Monto).FirstOrDefault();

                decimal minimoAceptado = pujaMaxima != null
                    ? pujaMaxima.Monto + subasta.Incremento_Minimo
                    : subasta.Precio_Base;

                if (request.Monto < minimoAceptado)
                {
                    return new ResultadoPujaDto
                    {
                        Exito = false,
                        MensajeError = $"El monto mínimo aceptado es ${minimoAceptado}.",
                        StatusCode = 400
                    };
                }

                var billeteraNueva = await _unitOfWork.Billeteras.GetByUsuarioIdAsync(request.UsuarioId);
                if (billeteraNueva == null)
                {
                    return new ResultadoPujaDto
                    {
                        Exito = false,
                        MensajeError = "Billetera no encontrada para el usuario.",
                        StatusCode = 404
                    };
                }

                decimal saldoDisponible = billeteraNueva.Saldo_Total - billeteraNueva.Saldo_Retenido;
                if (saldoDisponible < request.Monto)
                {
                    return new ResultadoPujaDto
                    {
                        Exito = false,
                        MensajeError = "Saldo insuficiente.",
                        StatusCode = 422
                    };
                }

                // --- Regla Anti-Sniping (Módulo 2.2) ---
                var tiempoRestante = subasta.Fecha_Fin - DateTime.UtcNow;
                if (tiempoRestante.TotalSeconds <= 60)
                {
                    subasta.Fecha_Fin = subasta.Fecha_Fin.AddMinutes(2);

                    await _unitOfWork.AuditoriaLogs.AddAsync(new Auditoria_Log
                    {
                        Entidad = "SUBASTA",
                        Entidad_Id = subasta.Id,
                        Accion = "EXTENSION_TIEMPO",
                        Usuario_Id = request.UsuarioId,
                        Detalle_Json = "{\"detalle\": \"Regla Anti-Sniping aplicada\"}",
                        Fecha = DateTime.UtcNow
                    });
                }

                // --- Retención del nuevo postor (Módulo 2.1) ---
                billeteraNueva.Saldo_Retenido += request.Monto;

                await _unitOfWork.Transacciones.AddAsync(new Transaccion_Ledger
                {
                    Billetera_Id = billeteraNueva.Id,
                    Tipo = "RETENCION",
                    Monto = request.Monto,
                    Fecha = DateTime.UtcNow,
                    Subasta_Id = subasta.Id
                });

                // --- Liberación automática del postor anterior (Módulo 2.1) ---
                if (pujaMaxima != null)
                {
                    var billeteraAnterior = await _unitOfWork.Billeteras.GetByUsuarioIdAsync(pujaMaxima.Comprador_Id);
                    if (billeteraAnterior != null)
                    {
                        billeteraAnterior.Saldo_Retenido -= pujaMaxima.Monto;

                        await _unitOfWork.Transacciones.AddAsync(new Transaccion_Ledger
                        {
                            Billetera_Id = billeteraAnterior.Id,
                            Tipo = "LIBERACION",
                            Monto = pujaMaxima.Monto,
                            Fecha = DateTime.UtcNow,
                            Subasta_Id = subasta.Id
                        });
                    }
                }

                var nuevaPuja = new Puja
                {
                    Subasta_Id = request.SubastaId,
                    Comprador_Id = request.UsuarioId,
                    Monto = request.Monto,
                    Fecha_Puja = DateTime.UtcNow
                };

                try
                {
                    nuevaPuja.ValidarDatos();
                }
                catch (ArgumentException ex)
                {
                    return new ResultadoPujaDto { Exito = false, MensajeError = ex.Message, StatusCode = 400 };
                }

                await _unitOfWork.Pujas.AddAsync(nuevaPuja);

                // Un único SaveChangesAsync: todo lo de arriba se confirma
                // (o se descarta) como una sola operación atómica.
                await _unitOfWork.SaveChangesAsync();

                return new ResultadoPujaDto { Exito = true, StatusCode = 200 };
            }
            catch (DbUpdateConcurrencyException)
            {
                // Optimistic Locking: otra puja tocó la misma Subasta/Billetera
                // entre que la leímos y la guardamos. Ningún cambio se aplicó.
                return new ResultadoPujaDto
                {
                    Exito = false,
                    MensajeError = "Conflicto de concurrencia: el estado de la subasta cambió. Intentá pujar de nuevo.",
                    StatusCode = 409
                };
            }
            catch (Exception)
            {
                return new ResultadoPujaDto
                {
                    Exito = false,
                    MensajeError = "Error interno del servidor.",
                    StatusCode = 500
                };
            }
        }
    }
}