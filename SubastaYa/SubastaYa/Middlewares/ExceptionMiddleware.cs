using System.Net;
using System.Text.Json;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Api.Middlewares
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
                // Petición hacia Controllers y Handlers
                await _next(context);
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Excepción atrapada por el Middleware global.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // El tipo de contenido para errores estandarizados
            context.Response.ContentType = "application/problem+json";

            int statusCode;
            string title;
            string detail = exception.Message;

            // Evaluación del tipo de excepción para asignar el Status Code correcto (REST Nivel 2)
            switch (exception)
            {
                // Error de Concurrencia Optimista -> 409 Conflict
                case ConcurrencyDomainException concurrencyEx:
                    statusCode = (int)HttpStatusCode.Conflict;
                    title = "Conflicto de estado";
                    detail = "Rechazo por concurrencia. Otro usuario acaba de pujar, por favor actualizá la subasta e intentá nuevamente.";
                    break;

                // Errores de negocio (Ej: Saldo insuficiente, mail duplicado) -> 400 Bad Request
                case DomainException domainEx:
                    statusCode = (int)HttpStatusCode.BadRequest; 
                    title = "Error de validación de negocio";
                    break;

                // Errores de solicitudes inválidas (como los de Identity) -> 400 Bad Request
                case BadHttpRequestException badRequestEx:
                    statusCode = (int)HttpStatusCode.BadRequest; 
                    title = "Solicitud incorrecta";
                    detail = badRequestEx.Message; 
                    break;

                // Errores de búsqueda (Ej: Subasta o Usuario no existe) -> 404 Not Found
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Recurso no encontrado";
                    break;

                // Errores de permisos -> 401 Unauthorized o 403 Forbidden
                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    title = "Acceso denegado";
                    // Utilizamos el mensaje personalizado ("Correo o contraseña incorrectos"), 
                    // y solo usamos el genérico si salta el error por defecto de .NET
                    detail = exception.Message.Contains("Attempted to perform")
                        ? "No tenés los permisos necesarios para realizar esta acción."
                        : exception.Message;
                    break;

                // Cualquier otro error inesperado -> 500 Internal Server Error
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    title = "Error Interno del Servidor";
                    detail = "Ha ocurrido un error inesperado. Por favor, contactá al soporte.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            // Armamos el JSON con el formato estándar RFC 7807
            var problemDetails = new
            {
                title = title,
                status = statusCode,
                detail = detail,
                instance = context.Request.Path
            };

            // Envio de respuesta
            var json = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(json);
        }
    }
}