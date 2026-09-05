namespace Application.UseCases.Auth.Commands
{
    public class RegistrarUsuarioCommand
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}