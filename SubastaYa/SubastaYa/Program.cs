using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Si pusiste la clase en una carpeta "Configuraciones", podés agregar el using acá:
// using SubastaYa.Configuraciones; 

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
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
    // (Asegurate de que el namespace "SubastaYa.Configuraciones" coincida con donde creaste la clase)
    c.SchemaFilter<SubastaYa.Configuraciones.SwaggerDefaultValuesFilter>();
});

// Registro de los Handlers de CQRS
builder.Services.AddScoped<Application.UseCases.Subastas.Handlers.RegistrarPujaCommandHandler>();
builder.Services.AddScoped<Application.UseCases.Usuarios.Handlers.GetMisActividadesQueryHandler>();
builder.Services.AddScoped<Application.UseCases.Wallet.Handlers.ObtenerHistorialQueryHandler>();
builder.Services.AddScoped<Application.UseCases.Wallet.Handlers.DepositarFondosCommandHandler>();

//Worker
builder.Services.AddHostedService<SubastaYa.Presentacion.Workers.SubastaCierreWorker>();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------------------------------------------------------------
// NUEVO: Registro de Repositorios y Unit of Work para la Arquitectura Limpia
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

// ¡Importante! Agregamos la autorización
builder.Services.AddAuthorization();

var app = builder.Build();

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

app.Run();