namespace SubastaYa.Application.UseCases.Subastas.CancelarSubasta
{
    public class CancelarSubastaCommand
    {
        public int SubastaId { get; set; }
        public int VendedorId { get; set; }
    }
}