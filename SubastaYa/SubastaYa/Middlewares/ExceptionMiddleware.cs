using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SubastaYa.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                // Atrapa nuestras reglas de negocio y devuelve HTTP 400
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Atrapa cualquier error inesperado y devuelve HTTP 500[cite: 2]
                _logger.LogError(ex, "Error no controlado en el servidor.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Ocurrió un error interno en el servidor." });
            }
        }
    }
}