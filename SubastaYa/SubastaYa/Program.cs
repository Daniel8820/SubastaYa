using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// using SubastaYa.Configuraciones; 

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SubastaYa API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorización JWT. Escribí 'Bearer [espacio] tu_token' en el cuadro de abajo.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // NUEVO: Agregamos el filtro para la fecha dinámica en el POST de Subastas
    c.SchemaFilter<SubastaYa.Configuraciones.SwaggerDefaultValuesFilter>();
});

// Registro de los Handlers de CQRS
builder.Services.AddScoped<Application.UseCases.Subastas.Handlers.CrearSubastaCommandHandler>();
builder.Services.AddScoped<Application.UseCases.Subastas.Handlers.GetCatalogoSubastasQueryHandler>();
builder.Services.AddScoped<Application.UseCases.Subastas.Handlers.GetSubastaByIdQueryHandler>();
builder.Services.AddScoped<Application.UseCases.Subastas.Handlers.RegistrarPujaCommandHandler>();

builder.Services.AddScoped<Application.UseCases.Usuarios.Handlers.GetMisActividadesQueryHandler>();
builder.Services.AddScoped<Application.UseCases.Usuarios.Handlers.RegistrarUsuarioCommandHandler>();

builder.Services.AddScoped<Application.UseCases.Wallet.Handlers.ConsultarSaldoQueryHandler>();
builder.Services.AddScoped<Application.UseCases.Wallet.Handlers.DepositarFondosCommandHandler>();
builder.Services.AddScoped<Application.UseCases.Wallet.Handlers.ObtenerHistorialQueryHandler>();

//Worker
builder.Services.AddHostedService<SubastaYa.Presentacion.Workers.SubastaCierreWorker>();

// Notificador
builder.Services.AddScoped<Application.Interfaces.INotificadorSubastas, Infrastructure.SignalR.NotificadorSubastas>();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de Identity
builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
{
    // Opciones de Contraseña Robustas
    options.Password.RequiredLength = 8;            // Mínimo de 8 caracteres
    options.Password.RequireUppercase = true;       // Al menos una letra mayúscula
    options.Password.RequireNonAlphanumeric = true; // Al menos un símbolo/carácter especial (!, @, #, etc)

    // (Opcional, pero muy recomendado en la industria)
    options.Password.RequireDigit = true;           // Al menos un número
    options.Password.RequireLowercase = true;       // Al menos una letra minúscula

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<Infrastructure.Persistence.SubastaYaDbContext>()
.AddDefaultTokenProviders();

// -------------------------------------------------------------------------
// Registro de Repositorios y Unit of Work para la Arquitectura Limpia
// -------------------------------------------------------------------------
builder.Services.AddScoped<Application.Interfaces.ISubastaRepository, Infrastructure.Persistence.Repositories.SubastaRepository>();
builder.Services.AddScoped<Application.Interfaces.IBilleteraRepository, Infrastructure.Persistence.Repositories.BilleteraRepository>();
builder.Services.AddScoped<Application.Interfaces.IUnitOfWork, Infrastructure.Persistence.UnitOfWork>();
// -------------------------------------------------------------------------

// Leemos la configuración del appsettings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidateAudience = true,
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Para que el token caduque exactamente a tiempo
    };
});

// Agregamos la autorización
builder.Services.AddAuthorization();

var app = builder.Build();

// Ejecutamos el Seeder Dinámico
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Llamamos a nuestro método estático de inicialización
        await Infrastructure.Persistence.DbInitializer.SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        // Si algo falla al cargar los datos, lo registramos en la consola
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al poblar la base de datos.");
    }
}

// Registramos nuestro middleware global de excepciones al inicio del pipeline
app.UseMiddleware<SubastaYa.Middlewares.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Infrastructure.SignalR.SubastaHub>("/hubs/subasta");

app.Run();