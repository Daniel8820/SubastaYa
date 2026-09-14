namespace SubastaYa.Application.DTOs
{
    public class RegistroPujaRequest
    {
        public int SubastaId { get; set; }
        public int CompradorId { get; set; }
        public decimal Monto { get; set; }
    }
}
