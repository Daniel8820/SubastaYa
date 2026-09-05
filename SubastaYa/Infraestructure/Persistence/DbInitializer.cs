using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            // Traemos las herramientas que necesitamos
            var context = serviceProvider.GetRequiredService<SubastaYaDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

            // 1. Cargamos las Categorías dinámicamente
            if (!await context.Categorias.AnyAsync())
            {
                context.Categorias.AddRange(
                    new Categoria { Nombre = "Tecnología", UrlIcono = "tech.png" },
                    new Categoria { Nombre = "Coleccionables", UrlIcono = "col.png" },
                    new Categoria { Nombre = "Indumentaria", UrlIcono = "ropa.png" },
                    new Categoria { Nombre = "Vehículos", UrlIcono = "auto.png" }
                );
                await context.SaveChangesAsync(); // Guardamos para que se generen los IDs
            }

            // 2. Cargamos los Usuarios y sus Billeteras usando Identity
            if (!await userManager.Users.AnyAsync())
            {
                var fechaBase = DateTime.UtcNow;

                var vendedor = new Usuario
                {
                    UserName = "vendedor@test.com",
                    Email = "vendedor@test.com",
                    Nombre = "Vendedor",
                    FechaRegistro = fechaBase.AddDays(-10),
                    Billetera = new Billetera { SaldoTotal = 0, SaldoRetenido = 0, SaldoDisponible = 0, Version = 1 }
                };
                await userManager.CreateAsync(vendedor, "Clave123!");

                var comp1 = new Usuario
                {
                    UserName = "comprador1@test.com",
                    Email = "comprador1@test.com",
                    Nombre = "Comprador 1",
                    FechaRegistro = fechaBase.AddDays(-5),
                    Billetera = new Billetera { SaldoTotal = 150000m, SaldoRetenido = 45000m, SaldoDisponible = 105000m, Version = 1 }
                };
                await userManager.CreateAsync(comp1, "Clave123!");

                var comp2 = new Usuario
                {
                    UserName = "comprador2@test.com",
                    Email = "comprador2@test.com",
                    Nombre = "Comprador 2",
                    FechaRegistro = fechaBase.AddDays(-2),
                    Billetera = new Billetera
                    {
                        SaldoTotal = 200000m,
                        SaldoRetenido = 25000m,
                        SaldoDisponible = 175000m,
                        Version = 1
                    }
                };
                await userManager.CreateAsync(comp2, "Clave123!");

                var sinFondos = new Usuario
                {
                    UserName = "sinfondos@test.com",
                    Email = "sinfondos@test.com",
                    Nombre = "Sin Fondos",
                    FechaRegistro = fechaBase.AddDays(-1),
                    Billetera = new Billetera { SaldoTotal = 500m, SaldoRetenido = 0m, SaldoDisponible = 500m, Version = 1 }
                };
                await userManager.CreateAsync(sinFondos, "Clave123!");
            }

            // 3. Cargamos Subastas, Pujas y Transacciones (Dependen de los IDs generados arriba)
            if (!await context.Subastas.AnyAsync())
            {
                var fechaBase = DateTime.UtcNow;

                // Buscamos a los usuarios que acabamos de crear para usar sus IDs reales
                var vendedor = await userManager.FindByEmailAsync("vendedor@test.com");
                var comp1 = await userManager.FindByEmailAsync("comprador1@test.com");
                var comp2 = await userManager.FindByEmailAsync("comprador2@test.com");

                var catTec = await context.Categorias.FirstAsync(c => c.Nombre == "Tecnología");
                var catCol = await context.Categorias.FirstAsync(c => c.Nombre == "Coleccionables");
                var catVeh = await context.Categorias.FirstAsync(c => c.Nombre == "Vehículos");
                var catInd = await context.Categorias.FirstAsync(c => c.Nombre == "Indumentaria");

                // Creamos los 5 casos de prueba
                var subasta1 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Notebook Pro", Descripcion = "Activa estándar", UrlImagen = "img1.png", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddMinutes(30), Estado = "ACTIVA", Version = 1 };
                var subasta2 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catCol.Id, Titulo = "Reloj Antiguo", Descripcion = "Activa crítica", UrlImagen = "img2.png", PrecioBase = 10000m, IncrementoMinimo = 500m, FechaInicio = fechaBase.AddHours(-2), FechaFin = fechaBase.AddMinutes(1), Estado = "ACTIVA", Version = 1 };
                var subasta3 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catVeh.Id, Titulo = "Auto Usado", Descripcion = "Inicia mañana", UrlImagen = "img3.png", PrecioBase = 1500000m, IncrementoMinimo = 50000m, FechaInicio = fechaBase.AddHours(24), FechaFin = fechaBase.AddHours(48), Estado = "PROGRAMADA", Version = 1 };
                var subasta4 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Monitor 24", Descripcion = "Para liquidar", UrlImagen = "img4.png", PrecioBase = 20000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddDays(-3), FechaFin = fechaBase.AddDays(-1), Estado = "ACTIVA", Version = 1 };
                var subasta5 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catInd.Id, Titulo = "Campera Cuero", Descripcion = "Nadie ofertó", UrlImagen = "img5.png", PrecioBase = 50000m, IncrementoMinimo = 2000m, FechaInicio = fechaBase.AddDays(-5), FechaFin = fechaBase.AddDays(-2), Estado = "ACTIVA", Version = 1 };

                context.Subastas.AddRange(subasta1, subasta2, subasta3, subasta4, subasta5);
                await context.SaveChangesAsync();

                // Creamos Pujas y Ledger enganchados a los IDs de arriba
                var puja1 = new Puja { SubastaId = subasta1.Id, CompradorId = comp1.Id, Monto = 35000m, FechaPuja = fechaBase.AddMinutes(-40) };
                var puja2 = new Puja { SubastaId = subasta1.Id, CompradorId = comp1.Id, Monto = 45000m, FechaPuja = fechaBase.AddMinutes(-20) };
                var puja3 = new Puja { SubastaId = subasta4.Id, CompradorId = comp2.Id, Monto = 25000m, FechaPuja = fechaBase.AddDays(-2) };
                context.Pujas.AddRange(puja1, puja2, puja3);

                var billeteraComp1 = await context.Billeteras.FirstAsync(b => b.UsuarioId == comp1.Id);
                var billeteraComp2 = await context.Billeteras.FirstAsync(b => b.UsuarioId == comp2.Id); // Buscamos la del comp2

                var t1 = new TransaccionLedger { BilleteraId = billeteraComp1.Id, Tipo = "DEPOSITO", Monto = 150000m, Fecha = fechaBase.AddDays(-4), SubastaId = null };
                var t2 = new TransaccionLedger { BilleteraId = billeteraComp1.Id, Tipo = "RETENCION", Monto = 45000m, Fecha = fechaBase.AddMinutes(-20), SubastaId = subasta1.Id };               
                var t3 = new TransaccionLedger { BilleteraId = billeteraComp2.Id, Tipo = "DEPOSITO", Monto = 200000m, Fecha = fechaBase.AddDays(-3), SubastaId = null };
                var t4 = new TransaccionLedger { BilleteraId = billeteraComp2.Id, Tipo = "RETENCION", Monto = 25000m, Fecha = fechaBase.AddDays(-2), SubastaId = subasta4.Id };

                context.TransaccionesLedger.AddRange(t1, t2, t3, t4);

                await context.SaveChangesAsync();
            }
        }
    }
}