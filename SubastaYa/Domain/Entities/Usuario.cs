namespace SubastaYa.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string IdentityId { get; set; }
        public Billetera Billetera { get; set; }
    }
}