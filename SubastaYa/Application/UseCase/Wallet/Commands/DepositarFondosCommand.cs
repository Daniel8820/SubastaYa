namespace Application.UseCases.Wallet.Commands
{
    public class DepositarFondosCommand
    {
        public int UsuarioId { get; set; }
        public decimal Monto { get; set; }
    }
}