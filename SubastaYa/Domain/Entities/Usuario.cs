using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    // Heredamos de IdentityUser<int> para que el ID siga siendo numérico
    public class Usuario : IdentityUser<int>
    {
        // El Id, Email y PasswordHash ya vienen incluidos de fábrica en IdentityUser.
        // Solo agregamos las propiedades extra que son de nuestro negocio:
        public string Nombre { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Relación 1:1 con Billetera
        public Billetera Billetera { get; set; }
    }
}
