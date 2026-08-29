namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string PasswordHash { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Relación 1:1 con Billetera
        public Billetera Billetera { get; set; }
    }
}
