using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface IUsuarioRepository
    {
        void Actualizar(Usuario usuario);
        Task AgregarAsync(Usuario usuario);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorIdentityIdAsync(string identityId);
    }
}