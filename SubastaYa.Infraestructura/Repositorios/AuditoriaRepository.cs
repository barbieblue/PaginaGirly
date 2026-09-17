using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Repositorios
{
    public class AuditoriaRepository : Repository<Auditoria_Log>, IAuditoriaRepository
    {
        public AuditoriaRepository(SubastaYaDbContext context) : base(context) { }
    }
}