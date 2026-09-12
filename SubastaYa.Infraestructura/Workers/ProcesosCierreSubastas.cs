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
    // BackgroundService es una clase base de .NET pensada exactamente para esto:
    // un proceso que corre en paralelo al servidor web, todo el tiempo que la
    // app esté prendida, revisando algo cada cierto tiempo.
    public class ProcesosCierreSubastas : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProcesosCierreSubastas> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(30);

        // OJO: el Worker es un "singleton" (una única instancia viva todo el
        // programa), pero el DbContext es "scoped" (una instancia por request).
        // Por eso no podemos inyectar IUnitOfWork directo en el constructor —
        // necesitamos un "fabricador de scopes" (IServiceScopeFactory) para
        // crear un scope nuevo cada vez que el Worker necesita hablar con la base.
        public ProcesosCierreSubastas(IServiceScopeFactory scopeFactory, ILogger<ProcesosCierreSubastas> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Este método arranca solo cuando la aplicación levanta, y corre en loop
        // hasta que la aplicación se apaga.
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
                    // Si algo sale mal en una vuelta, lo logueamos pero NO frenamos
                    // el Worker — tiene que seguir intentando en la próxima vuelta.
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

            // Buscamos subastas activas cuya fecha de fin ya pasó.
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
                    // Un SaveChangesAsync por subasta: si una falla (por ejemplo,
                    // por un conflicto de concurrencia con una puja que llegó
                    // justo en este instante), las demás subastas del lote
                    // siguen procesándose igual.
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
            // "Usuario_Id: null" porque este cambio lo ejecutó el Worker,
            // no una persona — así lo distinguimos en el log, como pide la consigna.
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