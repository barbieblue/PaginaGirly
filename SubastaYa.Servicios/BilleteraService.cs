using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Infraestructura;

namespace SubastaYa.Servicios
{
    // DTO simple para devolver el desglose de saldo pedido por la consigna.
    public class SaldoDto
    {
        public decimal SaldoTotal { get; set; }
        public decimal SaldoRetenido { get; set; }
        public decimal SaldoDisponible { get; set; }
    }

    public class BilleteraService
    {
        private readonly SubastaYaDbContext _context;

        public BilleteraService(SubastaYaDbContext context)
        {
            _context = context;
        }

        // Devuelve el desglose de saldo de un usuario puntual.
        public async Task<SaldoDto?> ObtenerSaldoAsync(int usuarioId)
        {
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.Usuario_Id == usuarioId);

            if (billetera == null)
            {
                return null; // el Controller decide qué código HTTP devolver en este caso
            }

            return new SaldoDto
            {
                SaldoTotal = billetera.Saldo_Total,
                SaldoRetenido = billetera.Saldo_Retenido,
                SaldoDisponible = billetera.Saldo_Disponible // la propiedad calculada que ya armamos
            };
        }

        // Acredita saldo simulado a la billetera de un usuario (según la consigna: "Carga de Saldo Simulada").
        public async Task<bool> DepositarAsync(int usuarioId, decimal monto)
        {
            if (monto <= 0)
            {
                return false; // el Controller devuelve 400 si esto falla
            }

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.Usuario_Id == usuarioId);

            if (billetera == null)
            {
                return false;
            }

            billetera.Saldo_Total += monto;

            // Dejamos registro del depósito en el libro contable, como pide la consigna
            // ("Historial de Movimientos: tabla con el detalle de ingresos...").
            _context.Transacciones.Add(new Transaccion_Ledger
            {
                Billetera_Id = billetera.Id,
                Tipo = "DEPOSITO",
                Monto = monto,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>?> ObtenerTransaccionesAsync(int usuarioId)
        {
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.Usuario_Id == usuarioId);

            if (billetera == null)
            {
                return null;
            }

            var transacciones = await _context.Transacciones
                .Where(t => t.Billetera_Id == billetera.Id)
                .OrderByDescending(t => t.Fecha)
                .Select(t => new
                {
                    t.Id,
                    t.Tipo,
                    t.Monto,
                    t.Fecha,
                    t.Subasta_Id
                })
                .ToListAsync<object>();

            return transacciones;
        }
    }

}