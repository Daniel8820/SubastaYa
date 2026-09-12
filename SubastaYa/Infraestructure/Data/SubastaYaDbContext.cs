using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Infrastructure.Data
{
    public class SubastaYaDbContext : IdentityDbContext<ApplicationUser>
    {
        public SubastaYaDbContext(DbContextOptions<SubastaYaDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Subasta> Subastas { get; set; }
        public DbSet<Billetera> Billeteras { get; set; }
        public DbSet<Puja> Pujas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<TransaccionLedger> TransaccionesLedger { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Llamar al base para que Identity genere las tablas de seguridad (Roles, Claims, etc.)
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().ToTable("Usuarios");

            // Marcar la propiedad Version como token de concurrencia
            modelBuilder.Entity<Subasta>().Property(s => s.Version).IsConcurrencyToken();
            modelBuilder.Entity<Billetera>().Property(b => b.Version).IsConcurrencyToken();

            // Precisión decimal
            modelBuilder.Entity<Billetera>().Property(b => b.SaldoTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Billetera>().Property(b => b.SaldoRetenido).HasPrecision(18, 2);
            modelBuilder.Entity<Billetera>().Property(b => b.SaldoDisponible).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.PrecioBase).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.IncrementoMinimo).HasPrecision(18, 2);
            modelBuilder.Entity<Puja>().Property(p => p.Monto).HasPrecision(18, 2);
            modelBuilder.Entity<TransaccionLedger>().Property(t => t.Monto).HasPrecision(18, 2);

            // Evitar borrado en cascada
            modelBuilder.Entity<Puja>()
                .HasOne(p => p.Comprador)
                .WithMany()
                .HasForeignKey(p => p.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subasta>()
                .HasOne(s => s.Vendedor)
                .WithMany()
                .HasForeignKey(s => s.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}