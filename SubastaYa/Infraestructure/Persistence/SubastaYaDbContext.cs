using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class SubastaYaDbContext : DbContext
    {
        public SubastaYaDbContext(DbContextOptions<SubastaYaDbContext> options) : base(options)
        {
        }

        // Definición de las tablas respetando PascalCasing
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Billetera> Billeteras { get; set; }
        public DbSet<Subasta> Subastas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Puja> Pujas { get; set; }
        public DbSet<TransaccionLedger> TransaccionesLedger { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración OBLIGATORIA de Optimistic Locking
            // Le decimos a EF Core que use el campo Version para manejar la concurrencia
            modelBuilder.Entity<Subasta>()
                .Property(s => s.Version)
                .IsConcurrencyToken();

            modelBuilder.Entity<Billetera>()
                .Property(b => b.Version)
                .IsConcurrencyToken();

            // 2. Precisión para los tipos de dato decimal (dinero)
            // Define la precisión en SQL Server mapeando a decimal(18,2) y evita truncamiento
            modelBuilder.Entity<Billetera>().Property(b => b.SaldoTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Billetera>().Property(b => b.SaldoRetenido).HasPrecision(18, 2);
            modelBuilder.Entity<Billetera>().Property(b => b.SaldoDisponible).HasPrecision(18, 2);

            modelBuilder.Entity<Subasta>().Property(s => s.PrecioBase).HasPrecision(18, 2);
            modelBuilder.Entity<Subasta>().Property(s => s.IncrementoMinimo).HasPrecision(18, 2);

            modelBuilder.Entity<Puja>().Property(p => p.Monto).HasPrecision(18, 2);

            modelBuilder.Entity<TransaccionLedger>().Property(t => t.Monto).HasPrecision(18, 2);

            // 3. Evitar el error de múltiples rutas en cascada de SQL Server
            modelBuilder.Entity<Puja>()
                .HasOne(p => p.Comprador)
                .WithMany()
                .HasForeignKey(p => p.CompradorId)
                .OnDelete(DeleteBehavior.Restrict); // Evita el borrado en cascada

            modelBuilder.Entity<Subasta>()
                .HasOne(s => s.Vendedor)
                .WithMany()
                .HasForeignKey(s => s.VendedorId)
                .OnDelete(DeleteBehavior.Restrict); // Evita el borrado en cascada

            // Definimos una fecha fija de referencia para evitar cambios constantes en las migraciones
            var fechaBase = new DateTime(2026, 8, 29, 12, 0, 0);

            // 1. Usuarios (Los 4 perfiles obligatorios)
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, Email = "vendedor@test.com", Nombre = "Vendedor", PasswordHash = "hash123", FechaRegistro = fechaBase.AddDays(-10) },
                new Usuario { Id = 2, Email = "comprador1@test.com", Nombre = "Comprador 1", PasswordHash = "hash123", FechaRegistro = fechaBase.AddDays(-5) },
                new Usuario { Id = 3, Email = "comprador2@test.com", Nombre = "Comprador 2", PasswordHash = "hash123", FechaRegistro = fechaBase.AddDays(-2) },
                new Usuario { Id = 4, Email = "sinfondos@test.com", Nombre = "Sin Fondos", PasswordHash = "hash123", FechaRegistro = fechaBase.AddDays(-1) }
            );

            // 2. Billeteras (Saldos exactos requeridos por el PDF)
            modelBuilder.Entity<Billetera>().HasData(
                new Billetera { Id = 1, UsuarioId = 1, SaldoTotal = 0m, SaldoRetenido = 0m, SaldoDisponible = 0m, Version = 1 },
                new Billetera { Id = 2, UsuarioId = 2, SaldoTotal = 150000m, SaldoRetenido = 45000m, SaldoDisponible = 105000m, Version = 1 },
                new Billetera { Id = 3, UsuarioId = 3, SaldoTotal = 200000m, SaldoRetenido = 0m, SaldoDisponible = 200000m, Version = 1 },
                new Billetera { Id = 4, UsuarioId = 4, SaldoTotal = 500m, SaldoRetenido = 0m, SaldoDisponible = 500m, Version = 1 }
            );

            // 3. Categorías 
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Tecnología", UrlIcono = "tech.png" },
                new Categoria { Id = 2, Nombre = "Coleccionables", UrlIcono = "col.png" },
                new Categoria { Id = 3, Nombre = "Indumentaria", UrlIcono = "ropa.png" },
                new Categoria { Id = 4, Nombre = "Vehículos", UrlIcono = "auto.png" }
            );

            // 4. Subastas (Los 5 casos de prueba)
            modelBuilder.Entity<Subasta>().HasData(
                // Caso 1: Activa estándar (Cierra en 30 min, líder en 45.000)
                new Subasta { Id = 1, VendedorId = 1, CategoriaId = 1, Titulo = "Notebook Pro", Descripcion = "Activa estándar", UrlImagen = "img1.png", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddMinutes(30), Estado = "ACTIVA", Version = 1 },

                // Caso 2: Activa crítica (Cierra en menos de 2 min para probar anti-sniping y color de alerta)
                new Subasta { Id = 2, VendedorId = 1, CategoriaId = 2, Titulo = "Reloj Antiguo", Descripcion = "Activa crítica", UrlImagen = "img2.png", PrecioBase = 10000m, IncrementoMinimo = 500m, FechaInicio = fechaBase.AddHours(-2), FechaFin = fechaBase.AddMinutes(1), Estado = "ACTIVA", Version = 1 },

                // Caso 3: Próxima (Inicia en 24hs)
                new Subasta { Id = 3, VendedorId = 1, CategoriaId = 4, Titulo = "Auto Usado", Descripcion = "Inicia mañana", UrlImagen = "img3.png", PrecioBase = 1500000m, IncrementoMinimo = 50000m, FechaInicio = fechaBase.AddHours(24), FechaFin = fechaBase.AddHours(48), Estado = "PROGRAMADA", Version = 1 },

                // Caso 4: Vencida con ganador (Para probar cierre y liquidación del worker)
                new Subasta { Id = 4, VendedorId = 1, CategoriaId = 1, Titulo = "Monitor 24", Descripcion = "Para liquidar", UrlImagen = "img4.png", PrecioBase = 20000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddDays(-3), FechaFin = fechaBase.AddDays(-1), Estado = "ACTIVA", Version = 1 },

                // Caso 5: Vencida desierta (Sin pujas registradas)
                new Subasta { Id = 5, VendedorId = 1, CategoriaId = 3, Titulo = "Campera Cuero", Descripcion = "Nadie ofertó", UrlImagen = "img5.png", PrecioBase = 50000m, IncrementoMinimo = 2000m, FechaInicio = fechaBase.AddDays(-5), FechaFin = fechaBase.AddDays(-2), Estado = "ACTIVA", Version = 1 }
            );

            // 5. Pujas y Ledger
            modelBuilder.Entity<Puja>().HasData(
                // 2 ofertas previas requeridas en la subasta activa
                new Puja { Id = 1, SubastaId = 1, CompradorId = 2, Monto = 35000m, FechaPuja = fechaBase.AddMinutes(-40) }, // Oferta anterior (superada)
                new Puja { Id = 2, SubastaId = 1, CompradorId = 2, Monto = 45000m, FechaPuja = fechaBase.AddMinutes(-20) }, // Oferta actual líder

                // Puja ganadora de la subasta vencida (asignada al comprador 2 para probar el worker)
                new Puja { Id = 3, SubastaId = 4, CompradorId = 3, Monto = 25000m, FechaPuja = fechaBase.AddDays(-2) }
            );

            modelBuilder.Entity<TransaccionLedger>().HasData(
                new TransaccionLedger { Id = 1, BilleteraId = 2, Tipo = "DEPOSITO", Monto = 150000m, Fecha = fechaBase.AddDays(-4), SubastaId = null },
                new TransaccionLedger { Id = 2, BilleteraId = 2, Tipo = "RETENCION", Monto = 45000m, Fecha = fechaBase.AddMinutes(-20), SubastaId = 1 }
            );

        }
    }
}