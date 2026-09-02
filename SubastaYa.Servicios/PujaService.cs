using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Infraestructura; // Asegúrate de usar el namespace de tu DbContext

namespace SubastaYa.Servicios
{
    // Clase auxiliar para devolver el resultado al controlador
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
            // 1. INICIAR TRANSACCIÓN ATÓMICA
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. OBTENER SUBASTA Y VALIDAR ESTADO
                var subasta = await _context.Subastas.FirstOrDefaultAsync(s => s.Id == subastaId);

                if (subasta == null || subasta.Estado != "ACTIVA" || subasta.FechaFin < DateTime.UtcNow)
                    return new ResultadoPuja { Exito = false, MensajeError = "Subasta no válida o finalizada.", StatusCode = 400 };

                // 3. VALIDAR MONTO Y OBTENER PUJA LÍDER ACTUAL
                var pujaMaxima = await _context.Pujas
                    .Where(p => p.SubastaId == subastaId)
                    .OrderByDescending(p => p.Monto)
                    .FirstOrDefaultAsync();

                decimal minimoAceptado = pujaMaxima != null ? pujaMaxima.Monto + subasta.IncrementoMinimo : subasta.PrecioBase;

                if (montoOfertado < minimoAceptado)
                    return new ResultadoPuja { Exito = false, MensajeError = $"El monto mínimo aceptado es ${minimoAceptado}.", StatusCode = 400 };

                // 4. VALIDAR SALDO DEL NUEVO POSTOR
                var billeteraNuevo = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

                // Calculamos el saldo disponible en memoria
                decimal saldoDisponible = billeteraNuevo.SaldoTotal - billeteraNuevo.SaldoRetenido;

                if (billeteraNuevo == null || saldoDisponible < montoOfertado)
                    return new ResultadoPuja { Exito = false, MensajeError = "Saldo insuficiente.", StatusCode = 422 }; // 422 como pide el diagrama

                // 5. REGLA ANTI-SNIPING (Extensión de tiempo)
                var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;
                if (tiempoRestante.TotalSeconds <= 60)
                {
                    subasta.FechaFin = subasta.FechaFin.AddMinutes(2);
                    _context.AuditoriaLogs.Add(new Auditoria_Log
                    {
                        Entidad = "SUBASTA",
                        EntidadId = subasta.Id,
                        Accion = "EXTENSION_TIEMPO",
                        Detalle = "Regla Anti-Sniping aplicada",
                        Fecha = DateTime.UtcNow
                    });
                }

                // 6. MANEJO ATÓMICO DE SALDOS (ESCROW)
                // Retener saldo al nuevo postor
                billeteraNuevo.SaldoRetenido += montoOfertado;
                _context.Transacciones.Add(new Transaccion_Ledger { BilleteraId = billeteraNuevo.Id, Tipo = "RETENCION", Monto = montoOfertado, Fecha = DateTime.UtcNow, SubastaId = subasta.Id });

                // Liberar saldo al postor superado (si existe)
                if (pujaMaxima != null)
                {
                    var billeteraAnterior = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == pujaMaxima.UsuarioId);
                    billeteraAnterior.SaldoRetenido -= pujaMaxima.Monto;
                    _context.Transacciones.Add(new Transaccion_Ledger { BilleteraId = billeteraAnterior.Id, Tipo = "LIBERACION", Monto = pujaMaxima.Monto, Fecha = DateTime.UtcNow, SubastaId = subasta.Id });
                }

                // 7. REGISTRAR LA NUEVA PUJA
                var nuevaPuja = new Puja { SubastaId = subastaId, UsuarioId = usuarioId, Monto = montoOfertado, FechaHora = DateTime.UtcNow };
                _context.Pujas.Add(nuevaPuja);

                // 8. GUARDAR CAMBIOS EN DB Y CONFIRMAR TRANSACCIÓN
                await _context.SaveChangesAsync(); // <-- ¡Aquí actúa el Optimistic Locking!
                await transaction.CommitAsync();

                return new ResultadoPuja { Exito = true, StatusCode = 200 };
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si dos usuarios pujan al mismo exacto milisegundo, la base de datos rechaza una.
                await transaction.RollbackAsync();
                return new ResultadoPuja { Exito = false, MensajeError = "Conflicto de concurrencia: El estado de la subasta cambió. Intenta pujar de nuevo.", StatusCode = 409 };
            }
            catch (Exception ex)
            {
                // Si falla cualquier otra cosa (ej. se cae la red a mitad del proceso)
                await transaction.RollbackAsync();
                return new ResultadoPuja { Exito = false, MensajeError = "Error interno del servidor.", StatusCode = 500 };
            }
        }
    }
}