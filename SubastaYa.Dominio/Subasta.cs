using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Dominio
{
    internal class Subasta
    {
        public int Id { get; set; }
        public int Vendedor_Id { get; set; }
        public Usuario Vendedor { get; set; }
        public int Categoria_Id { get; set; }
        public Categoria Categoria { get; set; }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Url_Imagen { get; set; }

        public decimal Precio_Base { get; set; }
        public decimal Incremento_Minimo { get; set; }

        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Fin { get; set; }

        public string Estado { get; set; } // PROGRAMADA, ACTIVA, FINALIZADA, DESIERTA

        public byte[] Version { get; set; } // optimistic locking
    }
}
