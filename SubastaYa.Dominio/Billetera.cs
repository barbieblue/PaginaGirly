using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio
{
    public class Billetera
    {
        public int Id { get; set; }
        public int Usuario_Id { get; set; }

        [ForeignKey("Usuario_Id")]
        public Usuario Usuario { get; set; }
        public decimal Saldo_Total { get; set; }
        public decimal Saldo_Retenido { get; set; }

        // Antes era una columna guardada a mano que nunca se actualizaba junto con
        // Saldo_Total / Saldo_Retenido y quedaba desincronizada. Ahora es una propiedad
        // calculada (no se mapea a una columna, ver DbContext.OnModelCreating) para que
        // siempre sea consistente con la formula del TP: Disponible = Total - Retenido.
        public decimal Saldo_Disponible => Saldo_Total - Saldo_Retenido;

        public byte[] Version { get; set; }  // para optimistic locking (RowVersion)
    }
}
