namespace SubastaYa.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        int UsuarioId { get; }
        string Nombre { get; }
    }
}