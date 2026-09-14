using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SubastaYa.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<SubastaYaDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>(); //Usamos el usuario de Infraestructura

            // Cargamos las Categorías dinámicamente
            if (!await context.Categorias.AnyAsync())
            {
                context.Categorias.AddRange(
                    new Categoria { Nombre = "Tecnología", UrlIcono = "tech.png" },
                    new Categoria { Nombre = "Coleccionables", UrlIcono = "col.png" },
                    new Categoria { Nombre = "Indumentaria", UrlIcono = "ropa.png" },
                    new Categoria { Nombre = "Vehículos", UrlIcono = "auto.png" },
                    new Categoria { Nombre = "Otros", UrlIcono = "otro.png" }
                );
                await context.SaveChangesAsync(); // Guardamos para que se generen los IDs
            }

            // Cargamos los Usuarios y sus Billeteras
            if (!await context.Usuarios.AnyAsync())
            {
                var fechaBase = DateTime.UtcNow;

                // Función para encapsular la creación de usuarios de prueba
                async Task CrearUsuarioPrueba(string email, string nombre, int diasOffset, decimal sTotal, decimal sRetenido, decimal sDisponible)
                {
                    // Creamos el login en Identity
                    var appUser = new ApplicationUser { UserName = email, Email = email };
                    await userManager.CreateAsync(appUser, "Clave123!");

                    // Nacemos la billetera limpia (los decimales en C# arrancan en 0 por defecto)
                    var nuevaBilletera = new Billetera
                    {
                        Version = 1
                    };

                    // Recreamos la historia financiera del usuario usando sus reglas de negocio
                    if (sTotal > 0)
                    {
                        // Esto sube SaldoTotal y SaldoDisponible
                        nuevaBilletera.Acreditar(sTotal);
                    }

                    if (sRetenido > 0)
                    {
                        // Esto descuenta del SaldoDisponible y lo pasa al SaldoRetenido
                        nuevaBilletera.RetenerFondos(sRetenido);
                    }

                    // Creamos la entidad de Dominio y anidamos la Billetera
                    var domainUser = new Usuario
                    {
                        Nombre = nombre,
                        Email = email,
                        FechaRegistro = fechaBase.AddDays(diasOffset),
                        IdentityId = appUser.Id,
                        Billetera = nuevaBilletera
                    };

                    await context.Usuarios.AddAsync(domainUser);
                }

                await CrearUsuarioPrueba("vendedor@test.com", "Vendedor", -10, 0, 0, 0);
                await CrearUsuarioPrueba("comprador1@test.com", "Comprador 1", -5, 150000m, 45000m, 105000m);
                await CrearUsuarioPrueba("comprador2@test.com", "Comprador 2", -2, 200000m, 25000m, 175000m);
                await CrearUsuarioPrueba("sinfondos@test.com", "Sin Fondos", -1, 500m, 0m, 500m);

                await context.SaveChangesAsync(); // Se guardan en la tabla del Dominio con sus Billeteras
            }

            // Cargamos Subastas, Pujas y Transacciones (Dependen de los IDs generados arriba)
            if (!await context.Subastas.AnyAsync())
            {
                var fechaBase = DateTime.UtcNow;

                // Buscamos a los usuarios en la tabla para tener sus IDs
                var vendedor = await context.Usuarios.FirstAsync(u => u.Email == "vendedor@test.com");
                var comp1 = await context.Usuarios.FirstAsync(u => u.Email == "comprador1@test.com");
                var comp2 = await context.Usuarios.FirstAsync(u => u.Email == "comprador2@test.com");

                var catTec = await context.Categorias.FirstAsync(c => c.Nombre == "Tecnología");
                var catCol = await context.Categorias.FirstAsync(c => c.Nombre == "Coleccionables");
                var catVeh = await context.Categorias.FirstAsync(c => c.Nombre == "Vehículos");
                var catInd = await context.Categorias.FirstAsync(c => c.Nombre == "Indumentaria");
                var catOtros = await context.Categorias.FirstAsync(c => c.Nombre == "Otros");


                // Creamos los casos de prueba (5 por default + agregados)
                var subasta1 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Notebook Pro", Descripcion = "Activa estándar", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/3/37/Schenker_VIA14_Laptop_asv2021-01.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddMinutes(30), Estado = "ACTIVA", Version = 1 };
                var subasta2 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catCol.Id, Titulo = "Reloj Antiguo", Descripcion = "Activa crítica", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/b/b0/Pocket_watch_photogaphed_through_the_viewfinder_of_a_Kodal_Duaflex.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 10000m, IncrementoMinimo = 500m, FechaInicio = fechaBase.AddHours(-2), FechaFin = fechaBase.AddMinutes(1), Estado = "ACTIVA", Version = 1 };
                var subasta3 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catVeh.Id, Titulo = "Auto Usado", Descripcion = "Inicia mañana", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/3/30/Car_at_West_Horsley_Place%2C_car_used_in_BBC_sitcom_Ghosts.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 1500000m, IncrementoMinimo = 50000m, FechaInicio = fechaBase.AddHours(24), FechaFin = fechaBase.AddHours(48), Estado = "PROGRAMADA", Version = 1 };
                var subasta4 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Monitor 24", Descripcion = "Para liquidar", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/d/d1/Monitor-233.svg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 20000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddDays(-3), FechaFin = fechaBase.AddDays(-1), Estado = "ACTIVA", Version = 1 };
                var subasta5 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catInd.Id, Titulo = "Campera Cuero", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/8/8d/Nepal_made_Leather_Jacket.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 50000m, IncrementoMinimo = 2000m, FechaInicio = fechaBase.AddDays(-5), FechaFin = fechaBase.AddDays(-2), Estado = "ACTIVA", Version = 1 };
                var subasta6 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Teclado Gamer", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/e/e0/Computer_keyboard_rainbow.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta7 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Mouse Gamer", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/7/71/2023_Mysz_komputerowa_Logitech_G903_Lightspeed.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta8 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catInd.Id, Titulo = "Anteojos", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/8/8f/Glasses_%D7%9E%D7%A9%D7%A7%D7%A4%D7%99%D7%99%D7%9D.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta9 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "Webcam", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/7/79/Webcam_%28Logitech_c922%29.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta10 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catTec.Id, Titulo = "CPU Gamer", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/0/04/MSI-Gaming-PC_2024-09-30.png?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta11 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catOtros.Id, Titulo = "Mate artesanal", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/4/40/Traditional_mate_set_calabash_gourd_with_La_Merced_yerba_mate.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta12 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catOtros.Id, Titulo = "Pesas", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/4/40/Dive_weights_-_500g_Bright_Weights_P8160082.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta13 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catOtros.Id, Titulo = "Almohada", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/5/54/Woven_pillow_%28%CF%83%CF%87%CE%AD%CE%B4%CE%B9%CE%BF_%CE%BA%CF%8D%CE%BA%CE%BB%CE%BF%CF%82%29.jpg?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta14 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catOtros.Id, Titulo = "Mesa", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/d/dd/Dinner_table_side_pine_wood_large_table_front.png?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };
                var subasta15 = new Subasta { VendedorId = vendedor.Id, CategoriaId = catOtros.Id, Titulo = "Taladro percutor", Descripcion = "Nadie ofertó", UrlImagen = "https://upload.wikimedia.org/wikipedia/commons/9/97/Milwaukee_Magnum_Holeshooter.png?utm_source=commons.wikimedia.org&utm_campaign=index&utm_content=original", PrecioBase = 30000m, IncrementoMinimo = 1000m, FechaInicio = fechaBase.AddHours(-1), FechaFin = fechaBase.AddHours(24), Estado = "ACTIVA", Version = 1 };

                context.Subastas.AddRange(subasta1, subasta2, subasta3, subasta4, subasta5, subasta6, subasta7, subasta8, subasta9, subasta10, subasta11, subasta12, subasta13, subasta14, subasta15);
                await context.SaveChangesAsync();

                // Creamos Pujas y Ledger en relación a los IDs de arriba
                var puja1 = new Puja { SubastaId = subasta1.Id, CompradorId = comp1.Id, Monto = 35000m, FechaPuja = fechaBase.AddMinutes(-40) };
                var puja2 = new Puja { SubastaId = subasta1.Id, CompradorId = comp1.Id, Monto = 45000m, FechaPuja = fechaBase.AddMinutes(-20) };
                var puja3 = new Puja { SubastaId = subasta4.Id, CompradorId = comp2.Id, Monto = 25000m, FechaPuja = fechaBase.AddDays(-2) };
                context.Pujas.AddRange(puja1, puja2, puja3);

                var billeteraComp1 = await context.Billeteras.FirstAsync(b => b.UsuarioId == comp1.Id);
                var billeteraComp2 = await context.Billeteras.FirstAsync(b => b.UsuarioId == comp2.Id);

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