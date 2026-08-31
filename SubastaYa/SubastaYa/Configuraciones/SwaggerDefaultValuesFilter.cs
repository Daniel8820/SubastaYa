using Application.Models; // Asegurate de usar tu namespace correcto
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SubastaYa.Configuraciones
{
    public class SwaggerDefaultValuesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Solo aplicamos esto a tu DTO de Crear Subasta
            if (context.Type == typeof(CrearSubastaRequest) && schema.Properties != null)
            {
                // Buscamos la propiedad fechaFin (Swagger la pone en minúscula)
                if (schema.Properties.ContainsKey("fechaFin"))
                {
                    // Calculamos la hora actual + 1 hora y le damos el formato limpio sin la "Z"
                    string fechaDefault = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss");
                    schema.Properties["fechaFin"].Example = new OpenApiString(fechaDefault);
                }
            }
        }
    }
}
