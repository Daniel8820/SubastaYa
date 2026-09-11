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
                // Dejamos que la petición siga su curso hacia los Controllers y Handlers
                await _next(context);
            }
            catch (Exception ex)
            {
                // Si algo explota en cualquier parte de la app, cae acá.
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

            // Evaluamos de qué tipo es la excepción para asignar el Status Code correcto (REST Nivel 2)
            switch (exception)
            {
                // 1. Errores de Negocio (Ej: Saldo insuficiente, mail duplicado) -> 400 Bad Request
                case DomainException domainEx:
                    statusCode = (int)HttpStatusCode.BadRequest; // 400
                    title = "Error de validación de negocio";
                    break;

                // 2. Capturamos los errores de solicitudes inválidas (como los de Identity) -> 400 Bad Request
                case BadHttpRequestException badRequestEx:
                    statusCode = (int)HttpStatusCode.BadRequest; // 400
                    title = "Solicitud incorrecta";
                    detail = badRequestEx.Message; // Mantiene los mensajes exactos de Identity (ej: longitud de clave, símbolos, etc.)
                    break;

                // 3. Errores de Búsqueda (Ej: Subasta o Usuario no existe) -> 404 Not Found
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound; // 404
                    title = "Recurso no encontrado";
                    break;

                // 4. Errores de Permisos -> 401 Unauthorized o 403 Forbidden
                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Forbidden; // 403
                    title = "Acceso denegado";
                    detail = "No tenés los permisos necesarios para realizar esta acción.";
                    break;

                // 5. Cualquier otra cosa inesperada -> 500 Internal Server Error
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError; // 500
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
                // Podés sumar la URL que generó el error para dar más contexto
                instance = context.Request.Path
            };

            // Serializamos y enviamos la respuesta al cliente
            var json = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(json);
        }
    }
}