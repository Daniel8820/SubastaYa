namespace SubastaYa.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> RegistrarLoginAsync(string email, string password);
        Task<bool> CambiarPasswordAsync(string identityId, string passwordActual, string nuevaPassword);
        Task<string> ValidarCredencialesAsync(string email, string password);
    }
}