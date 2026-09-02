using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio
{
    public class Auditoria_Log
    {
        public int Id { get; set; }
        public string Entidad { get; set; }

        
        public int? Entidad_Id { get; set; }
        public string Accion { get; set; }

       
        public int? Usuario_Id { get; set; }
        [ForeignKey("Usuario_Id")]
        public Usuario Usuario { get; set; }

        public string Detalle_Json { get; set; }
        public DateTime Fecha { get; set; }
    }
}
