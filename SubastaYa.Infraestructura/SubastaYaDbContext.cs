using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio;

namespace SubastaYa.Infraestructura
{
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

        // Método donde se configura el modelo de datos mediante Fluent API (restricciones, índices y mapeos).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Optimistic Locking real ----
            // Configura el campo Version como RowVersion en SQL Server para evitar sobrescrituras simultáneas en Subastas.
            modelBuilder.Entity<Subasta>()
                .Property(s => s.Version)
                .IsRowVersion();

            // Configura el campo Version como RowVersion para el control de concurrencia en Billeteras.
            modelBuilder.Entity<Billetera>()
                .Property(b => b.Version)
                .IsRowVersion();

            // Saldo_Disponible ahora es una propiedad calculada en memoria (Total - Retenido)
            // y se indica a EF Core que la ignore para que no intente buscarla ni crearla como columna en la base de datos.
            modelBuilder.Entity<Billetera>()
                .Ignore(b => b.Saldo_Disponible);

            // ---- Precisión de columnas monetarias ----
            // Define el tipo decimal(18, 2) para evitar errores de redondeo en campos monetarios y precios.
            modelBuilder.Entity<Billetera>().Property(b => b.Saldo_Total).HasPrecision(18, 2);
            modelBuilder.Entity<Billetera>().Property(b => b.Saldo_Retenido).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.Precio_Base).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.Incremento_Minimo).HasPrecision(18, 2);
            modelBuilder.Entity<Puja>().Property(p => p.Monto).HasPrecision(18, 2);
            modelBuilder.Entity<Transaccion_Ledger>().Property(t => t.Monto).HasPrecision(18, 2);

            // ---- Restricciones de integridad ----
            // Crea un índice único en el Email para impedir usuarios duplicados.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Crea un índice único en Usuario_Id para asegurar una relación estrictamente uno a uno con la Billetera.
            modelBuilder.Entity<Billetera>()
                .HasIndex(b => b.Usuario_Id)
                .IsUnique();

            // Evitar borrados en cascada accidentales sobre historial contable / de auditoría en la relación de Puja y Subasta.
            modelBuilder.Entity<Puja>()
                .HasOne(p => p.Subasta)
                .WithMany()
                .HasForeignKey(p => p.Subasta_Id);
        }
    }
}