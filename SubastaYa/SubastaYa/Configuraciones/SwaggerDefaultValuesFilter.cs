// Asegurate de importar el namespace del Command
using Application.UseCases.Subastas.Commands;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SubastaYa.Configuraciones
{
    public class SwaggerDefaultValuesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // CAMBIO ACÁ: Usamos CrearSubastaCommand
            if (context.Type == typeof(CrearSubastaCommand) && schema.Properties != null)
            {
                if (schema.Properties.ContainsKey("fechaFin"))
                {
                    string fechaDefault = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss");
                    schema.Properties["fechaFin"].Example = new OpenApiString(fechaDefault);
                }
            }
        }
    }
}