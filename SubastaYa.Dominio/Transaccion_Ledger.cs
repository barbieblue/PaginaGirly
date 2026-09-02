using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio
{
    public class Transaccion_Ledger
    {
        public int Id { get; set; }

        
        public int Billetera_Id { get; set; }
        [ForeignKey("Billetera_Id")]
        public Billetera Billetera { get; set; }

        public string Tipo { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }

       
        public int? Subasta_Id { get; set; }
        [ForeignKey("Subasta_Id")]
        public Subasta Subasta { get; set; }
    }
}
