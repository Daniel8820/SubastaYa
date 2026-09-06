namespace Application.UseCases.Usuarios.Commands
{
    public class CambiarPasswordCommand
    {
        public int UsuarioId { get; set; }
        public string PasswordActual { get; set; }
        public string PasswordNueva { get; set; }
    }
}