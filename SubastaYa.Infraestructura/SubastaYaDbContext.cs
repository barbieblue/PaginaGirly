using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;
using SubastaYa.Infraestructura;

namespace SubastaYa.Infraestructura
{
    public class SubastaYaDbContext : DbContext
    {
        public SubastaYaDbContext(DbContextOptions<SubastaYaDbContext> options)
            : base(options) { }

        // Cada DbSet<T> = una tabla en la base de datos
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Billetera> Billeteras { get; set; }
        public DbSet<Subasta> Subastas { get; set; }
        public DbSet<Puja> Pujas { get; set; }
        public DbSet<Transaccion_Ledger> Transacciones { get; set; }
        public DbSet<Auditoria_Log> AuditoriaLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subasta>()
                .Property(s => s.Version)
                .IsRowVersion();

            modelBuilder.Entity<Billetera>()
                .Property(b => b.Version)
                .IsRowVersion();

            modelBuilder.Entity<Billetera>()
                .Ignore(b => b.Saldo_Disponible);

            modelBuilder.Entity<Billetera>().Property(b => b.Saldo_Total).HasPrecision(18, 2);
            modelBuilder.Entity<Billetera>().Property(b => b.Saldo_Retenido).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.Precio_Base).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.Incremento_Minimo).HasPrecision(18, 2);
            modelBuilder.Entity<Puja>().Property(p => p.Monto).HasPrecision(18, 2);
            modelBuilder.Entity<Transaccion_Ledger>().Property(t => t.Monto).HasPrecision(18, 2);

            // --- REGLAS DE INTEGRIDAD ---
            // No permite dos usuarios con el mismo email.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Un usuario solo puede tener UNA billetera (relación 1 a 1 real).
            modelBuilder.Entity<Billetera>()
                .HasIndex(b => b.Usuario_Id)
                .IsUnique();

            modelBuilder.Entity<Puja>()
                .HasOne(p => p.Comprador)
                .WithMany()
                .HasForeignKey(p => p.Comprador_Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaccion_Ledger>()
                .HasOne(t => t.Subasta)
                .WithMany()
                .HasForeignKey(t => t.Subasta_Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

