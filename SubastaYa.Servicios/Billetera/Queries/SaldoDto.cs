using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SubastaYa.Servicios.Billetera.Queries
{
    // Módulo 4 (Panel de Saldo): las tres métricas que pide la consigna.
    public class SaldoDto
    {
        public decimal SaldoTotal { get; set; }
        public decimal SaldoRetenido { get; set; }
        public decimal SaldoDisponible { get; set; }
    }
}