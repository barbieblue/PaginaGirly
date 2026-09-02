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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Evita el ciclo: al borrar un Usuario, no cascadear el borrado de sus Pujas
        // (la cascada por Subasta -> Puja sí se mantiene).
        modelBuilder.Entity<Puja>()
            .HasOne(p => p.Comprador)
            .WithMany()
            .HasForeignKey(p => p.Comprador_Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Evita el ciclo: al borrar un Usuario, no cascadear el borrado de Transacciones
        // vía Subasta (la cascada por Billetera -> Transaccion sí se mantiene).
        modelBuilder.Entity<Transaccion_Ledger>()
            .HasOne(t => t.Subasta)
            .WithMany()
            .HasForeignKey(t => t.Subasta_Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}