using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Workers
{
    public class ProcesosCierreSubastas : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProcesosCierreSubastas> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(30);

        public ProcesosCierreSubastas(IServiceScopeFactory scopeFactory, ILogger<ProcesosCierreSubastas> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarSubastasVencidasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar subastas vencidas.");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private async Task ProcesarSubastasVencidasAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var ahora = DateTime.UtcNow;

            var subastasVencidas = await unitOfWork.Subastas
                .Consultar()
                .Where(s => s.Estado == "ACTIVA" && s.Fecha_Fin <= ahora)
                .ToListAsync();

            foreach (var subasta in subastasVencidas)
            {
                var pujas = await unitOfWork.Pujas.GetBySubastaIdAsync(subasta.Id);

                if (pujas.Any())
                {
                    await AdjudicarConGanadorAsync(unitOfWork, subasta, pujas, ahora);
                }
                else
                {
                    await MarcarDesiertaAsync(unitOfWork, subasta, ahora);
                }

                try
                {
                    await unitOfWork.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Conflicto de concurrencia al cerrar la subasta {Id}", subasta.Id);
                }
            }
        }

        private async Task AdjudicarConGanadorAsync(IUnitOfWork unitOfWork, Subasta subasta, List<Puja> pujas, DateTime ahora)
        {
            var pujaGanadora = pujas.OrderByDescending(p => p.Monto).First();

            subasta.Estado = "FINALIZADA";
            unitOfWork.Subastas.Update(subasta);

            var billeteraComprador = await unitOfWork.Billeteras.GetByUsuarioIdAsync(pujaGanadora.Comprador_Id);
            var billeteraVendedor = await unitOfWork.Billeteras.GetByUsuarioIdAsync(subasta.Vendedor_Id);

            if (billeteraComprador != null && billeteraVendedor != null)
            {
                // Liquidación final: se descuenta lo retenido del comprador
                // (ya no está "reservado", se convirtió en un pago real) y se
                // acredita al vendedor.
                billeteraComprador.Saldo_Total -= pujaGanadora.Monto;
                billeteraComprador.Saldo_Retenido -= pujaGanadora.Monto;
                billeteraVendedor.Saldo_Total += pujaGanadora.Monto;

                unitOfWork.Billeteras.Update(billeteraComprador);
                unitOfWork.Billeteras.Update(billeteraVendedor);

                await unitOfWork.Transacciones.AddAsync(new Transaccion_Ledger
                {
                    Billetera_Id = billeteraComprador.Id,
                    Tipo = "PAGO",
                    Monto = pujaGanadora.Monto,
                    Fecha = ahora,
                    Subasta_Id = subasta.Id
                });

                await unitOfWork.Transacciones.AddAsync(new Transaccion_Ledger
                {
                    Billetera_Id = billeteraVendedor.Id,
                    Tipo = "COBRO",
                    Monto = pujaGanadora.Monto,
                    Fecha = ahora,
                    Subasta_Id = subasta.Id
                });
            }

            await RegistrarAuditoriaAsync(unitOfWork, subasta.Id, "CIERRE_CON_GANADOR",
                $"Subasta finalizada. Ganador: usuario {pujaGanadora.Comprador_Id}, monto {pujaGanadora.Monto}.");
        }

        private async Task MarcarDesiertaAsync(IUnitOfWork unitOfWork, Subasta subasta, DateTime ahora)
        {
            subasta.Estado = "DESIERTA";
            unitOfWork.Subastas.Update(subasta);

            await RegistrarAuditoriaAsync(unitOfWork, subasta.Id, "CIERRE_DESIERTA",
                "Subasta cerrada sin ofertas.");
        }

        private async Task RegistrarAuditoriaAsync(IUnitOfWork unitOfWork, int subastaId, string accion, string detalle)
        {
            await unitOfWork.AuditoriaLogs.AddAsync(new Auditoria_Log
            {
                Entidad = "SUBASTA",
                Entidad_Id = subastaId,
                Accion = accion,
                Usuario_Id = null,
                Detalle_Json = detalle,
                Fecha = DateTime.UtcNow
            });
        }
    }
}