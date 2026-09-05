using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<bool> ExisteEmailAsync(string email);
        void Agregar(Usuario usuario);
    }
}