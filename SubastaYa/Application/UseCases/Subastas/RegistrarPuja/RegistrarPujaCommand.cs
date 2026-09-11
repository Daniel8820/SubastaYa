namespace SubastaYa.Application.UseCases.Subastas.RegistrarPuja
{
    public class RegistrarPujaCommand
    {
        public int SubastaId { get; set; }
        public int CompradorId { get; set; }
        public string? CompradorNombre { get; set; }
        public decimal Monto { get; set; }
    }
}