using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Servicios.Billetera.Commands
{
    public class DepositarResultadoDto
    {
        public bool Exito { get; set; }
        public string? MensajeError { get; set; }
        public decimal? SaldoTotalActualizado { get; set; }
    }
}