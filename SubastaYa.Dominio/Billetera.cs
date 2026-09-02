using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio
{
    public class Billetera
    {
        public int Id { get; set; }
        public int Usuario_Id { get; set; }
        public Usuario Usuario { get; set; }
        public decimal Saldo_Total { get; set; }
        public decimal Saldo_Retenido { get; set; }
        public decimal Saldo_Disponible { get; set; }
        public byte[] Version { get; set; }  // para optimistic locking (RowVersion)
    }
}
