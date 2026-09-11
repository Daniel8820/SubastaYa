namespace SubastaYa.Application.UseCases.Wallet.DepositarFondos
{
    public class DepositarFondosCommand
    {
        public int UsuarioId { get; set; }
        public decimal Monto { get; set; }
    }
}