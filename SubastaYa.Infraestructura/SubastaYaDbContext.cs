using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;

namespace SubastaYa.Infraestructura;

public class SubastaYaDbContext : DbContext
{
    public SubastaYaDbContext(DbContextOptions<SubastaYaDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Billetera> Billeteras { get; set; }
    public DbSet<Subasta> Subastas { get; set; }
    public DbSet<Puja> Pujas { get; set; }
    public DbSet<Transaccion_Ledger> Transacciones { get; set; }
    
   public DbSet<Auditoria_Log> AuditoriaLogs { get; set; }
}
