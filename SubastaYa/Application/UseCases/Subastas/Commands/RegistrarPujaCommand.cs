namespace Application.UseCases.Subastas.Commands
{
    public class RegistrarPujaCommand
    {
        public int SubastaId { get; set; }
        public int CompradorId { get; set; }
        public decimal Monto { get; set; }
    }
}