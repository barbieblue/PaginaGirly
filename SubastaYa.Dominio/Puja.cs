using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio
{
    public class Puja
    {
        public int Id { get; set; }

        public int Subasta_Id { get; set; }
        [ForeignKey("Subasta_Id")]
        public Subasta Subasta { get; set; }

        public int Comprador_Id { get; set; }
        [ForeignKey("Comprador_Id")]
        public Usuario Comprador { get; set; }

        public decimal Monto { get; set; }
        public DateTime Fecha_Puja { get; set; }
    }
}