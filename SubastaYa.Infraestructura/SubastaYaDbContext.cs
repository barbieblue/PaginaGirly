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

        // Acá se afinan reglas del modelo que los atributos de las clases no pueden expresar solos.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- OPTIMISTIC LOCKING (requisito del TP) ---
            // Le dice a SQL Server: "esta columna Version se actualiza sola, automáticamente,
            // cada vez que alguien modifica la fila". Si dos personas leen la misma Subasta al
            // mismo tiempo e intentan guardar cambios, la segunda va a chocar contra un
            // "Version" viejo y EF Core tira DbUpdateConcurrencyException -> vos lo convertís
            // en el 409 Conflict que pide la consigna.
            modelBuilder.Entity<Subasta>()
                .Property(s => s.Version)
                .IsRowVersion();

            modelBuilder.Entity<Billetera>()
                .Property(b => b.Version)
                .IsRowVersion();

            // --- CAMPO CALCULADO ---
            // Saldo_Disponible no es un dato que se guarda; se calcula solo (Total - Retenido,
            // ver Billetera.cs). Esta línea le dice a EF "no crees columna para esto, ignoralo".
            modelBuilder.Entity<Billetera>()
                .Ignore(b => b.Saldo_Disponible);

            // --- PRECISIÓN DE NÚMEROS CON DECIMALES ---
            // decimal(18,2) = hasta 18 dígitos en total, 2 después de la coma.
            // Sin esto, SQL Server podría redondear mal montos de dinero.
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

            // --- EVITAR CICLOS DE BORRADO EN CASCADA ---
            // (Ya vimos este problema antes: si dos caminos de cascada llegan a la misma
            // tabla, SQL Server rechaza crear la base. Acá cortamos el camino "extra".)

            // Camino cortado: Usuario (comprador) -> Puja
            modelBuilder.Entity<Puja>()
                .HasOne(p => p.Comprador)
                .WithMany()
                .HasForeignKey(p => p.Comprador_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Camino cortado: Usuario -> Subasta -> Transaccion (vía Subasta)
            modelBuilder.Entity<Transaccion_Ledger>()
                .HasOne(t => t.Subasta)
                .WithMany()
                .HasForeignKey(t => t.Subasta_Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

