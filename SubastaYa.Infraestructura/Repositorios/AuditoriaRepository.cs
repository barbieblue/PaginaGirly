using SubastaYa.Dominio;
using SubastaYa.Dominio.Repositorios;

namespace SubastaYa.Infraestructura.Repositorios
{
    public class AuditoriaRepository : Repository<Auditoria_Log>, IAuditoriaRepository
    {
        public AuditoriaRepository(SubastaYaDbContext context) : base(context) { }
    }
}