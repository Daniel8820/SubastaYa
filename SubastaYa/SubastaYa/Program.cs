using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.UseCases.Subastas.ActivarSubastas;
using SubastaYa.Application.UseCases.Subastas.CancelarSubasta;
using SubastaYa.Application.UseCases.Subastas.CrearSubasta;
using SubastaYa.Application.UseCases.Subastas.FinalizarSubastas;
using SubastaYa.Application.UseCases.Subastas.GetCatalogoSubastas;
using SubastaYa.Application.UseCases.Subastas.GetSubastaById;
using SubastaYa.Application.UseCases.Subastas.RegistrarPuja;
using SubastaYa.Application.UseCases.Usuarios.ActualizarPerfil;
using SubastaYa.Application.UseCases.Usuarios.CambiarPassword;
using SubastaYa.Application.UseCases.Usuarios.GetMisActividades;
using SubastaYa.Application.UseCases.Usuarios.RegistrarUsuario;
using SubastaYa.Application.UseCases.Wallet.ConsultarSaldo;
using SubastaYa.Application.UseCases.Wallet.DepositarFondos;
using SubastaYa.Application.UseCases.Wallet.ObtenerHistorial;
using SubastaYa.Infrastructure.Data;
using SubastaYa.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddSignalR();

// Configuración de CORS para permitir peticiones desde React
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // El puerto por defecto de Vite
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Para que SignalR funcione con WebSockets
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SubastaYa API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorización JWT. 'Bearer [espacio] token' en el cuadro de abajo.",
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
});

// Registro de los Handlers de CQRS
builder.Services.AddScoped<CrearSubastaCommandHandler>();
builder.Services.AddScoped<GetCatalogoSubastasQueryHandler>();
builder.Services.AddScoped<GetSubastaByIdQueryHandler>();
builder.Services.AddScoped<RegistrarPujaCommandHandler>();
builder.Services.AddScoped<CancelarSubastaCommandHandler>();
builder.Services.AddScoped<ActivarSubastasCommandHandler>();
builder.Services.AddScoped<FinalizarSubastasCommandHandler>();


builder.Services.AddScoped<GetMisActividadesQueryHandler>();
builder.Services.AddScoped<RegistrarUsuarioCommandHandler>();
builder.Services.AddScoped<ActualizarPerfilCommandHandler>();
builder.Services.AddScoped<CambiarPasswordCommandHandler>();

builder.Services.AddScoped<ConsultarSaldoQueryHandler>();
builder.Services.AddScoped<DepositarFondosCommandHandler>();
builder.Services.AddScoped<ObtenerHistorialQueryHandler>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registramos el puente de autenticación de Identity
builder.Services.AddScoped<SubastaYa.Application.Interfaces.Services.IAuthService, SubastaYa.Infrastructure.Services.AuthService>();

//Worker
builder.Services.AddHostedService<SubastaYa.Infrastructure.Workers.SubastaBackgroundWorker>();

// Notificador
builder.Services.AddScoped<INotificadorSubastas, SubastaYa.Infrastructure.SignalR.NotificadorSubastas>();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de Identity
builder.Services.AddIdentity<SubastaYa.Infrastructure.Identity.ApplicationUser, IdentityRole>(options =>
{
    // Opciones de Contraseña Robustas
    options.Password.RequiredLength = 8;            // Mínimo de 8 caracteres
    options.Password.RequireUppercase = true;       // Al menos una letra mayúscula
    options.Password.RequireNonAlphanumeric = true; // Al menos un símbolo/carácter especial (!, @, #, etc)
    options.Password.RequireDigit = true;           // Al menos un número
    options.Password.RequireLowercase = true;       // Al menos una letra minúscula
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SubastaYaDbContext>()
.AddDefaultTokenProviders();

// -------------------------------------------------------------------------
// Registro de Repositorios y Unit of Work para la Arquitectura Limpia
// -------------------------------------------------------------------------
builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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
        await DbInitializer.SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        // Si algo falla al cargar los datos, lo registramos en la consola
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al poblar la base de datos.");
    }
}

// Registramos nuestro middleware global de excepciones al inicio del pipeline
app.UseMiddleware<SubastaYa.Api.Middlewares.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SubastaYa.Infrastructure.SignalR.SubastaHub>("/hubs/subasta");

app.Run();