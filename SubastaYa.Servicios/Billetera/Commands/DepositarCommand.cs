using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Commands
{
    // Módulo 4: "Carga de Saldo Simulada".
    public class DepositarCommand : IRequest<DepositarResultadoDto>
    {
        public int UsuarioId { get; set; }
        public decimal Monto { get; set; }
    }
}
