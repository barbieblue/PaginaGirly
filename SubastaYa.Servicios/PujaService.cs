using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Infraestructura;

namespace SubastaYa.Servicios
{
    public class ResultadoPuja
    {
        public bool Exito { get; set; }
        public string MensajeError { get; set; }
        public int StatusCode { get; set; }
    }

    public class PujaService
    {
        private readonly SubastaYaDbContext _context;

        public PujaService(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPuja> RegistrarPujaAsync(int subastaId, int usuarioId, decimal montoOfertado)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var subasta = await _context.Subastas.FirstOrDefaultAsync(s => s.Id == subastaId);

                if (subasta == null || subasta.Estado != "ACTIVA" || subasta.Fecha_Fin < DateTime.UtcNow)
                    return new ResultadoPuja { Exito = false, MensajeError = "Subasta no válida o finalizada.", StatusCode = 400 };

                var pujaMaxima = await _context.Pujas
                    .Where(p => p.Subasta_Id == subastaId)
                    .OrderByDescending(p => p.Monto)
                    .FirstOrDefaultAsync();

                decimal minimoAceptado = pujaMaxima != null ? pujaMaxima.Monto + subasta.Incremento_Minimo : subasta.Precio_Base;

                if (montoOfertado < minimoAceptado)
                    return new ResultadoPuja { Exito = false, MensajeError = $"El monto mínimo aceptado es ${minimoAceptado}.", StatusCode = 400 };

                var billeteraNuevo = await _context.Billeteras.FirstOrDefaultAsync(b => b.Usuario_Id == usuarioId);

                if (billeteraNuevo == null)
                    return new ResultadoPuja { Exito = false, MensajeError = "Billetera no encontrada para el usuario.", StatusCode = 404 };

                decimal saldoDisponible = billeteraNuevo.Saldo_Total - billeteraNuevo.Saldo_Retenido;

                if (saldoDisponible < montoOfertado)
                    return new ResultadoPuja { Exito = false, MensajeError = "Saldo insuficiente.", StatusCode = 422 };

                var tiempoRestante = subasta.Fecha_Fin - DateTime.UtcNow;
                if (tiempoRestante.TotalSeconds <= 60)
                {
                    subasta.Fecha_Fin = subasta.Fecha_Fin.AddMinutes(2);

                    _context.AuditoriaLogs.Add(new Auditoria_Log
                    {
                        Entidad = "SUBASTA",
                        Entidad_Id = subasta.Id,
                        Accion = "EXTENSION_TIEMPO",
                        Usuario_Id = usuarioId,
                        Detalle_Json = "{\"detalle\": \"Regla Anti-Sniping aplicada\"}",
                        Fecha = DateTime.UtcNow
                    });
                }

                billeteraNuevo.Saldo_Retenido += montoOfertado;

                _context.Transacciones.Add(new Transaccion_Ledger
                {
                    Billetera_Id = billeteraNuevo.Id,
                    Tipo = "RETENCION",
                    Monto = montoOfertado,
                    Fecha = DateTime.UtcNow,
                    Subasta_Id = subasta.Id
                });

                if (pujaMaxima != null)
                {
                    var billeteraAnterior = await _context.Billeteras.FirstOrDefaultAsync(b => b.Usuario_Id == pujaMaxima.Comprador_Id);
                    if (billeteraAnterior != null)
                    {
                        billeteraAnterior.Saldo_Retenido -= pujaMaxima.Monto;
                        _context.Transacciones.Add(new Transaccion_Ledger
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
                    Subasta_Id = subastaId,
                    Comprador_Id = usuarioId,
                    Monto = montoOfertado,
                    Fecha_Puja = DateTime.UtcNow
                };
                _context.Pujas.Add(nuevaPuja);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultadoPuja { Exito = true, StatusCode = 200 };
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return new ResultadoPuja { Exito = false, MensajeError = "Conflicto de concurrencia: El estado de la subasta cambió. Intenta pujar de nuevo.", StatusCode = 409 };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new ResultadoPuja { Exito = false, MensajeError = "Error interno del servidor.", StatusCode = 500 };
            }
        }
    }
}