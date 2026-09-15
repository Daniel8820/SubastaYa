using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface IUsuarioRepository
    {
        void Actualizar(Usuario usuario);
        Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
        Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<Usuario?> ObtenerPorIdentityIdAsync(string identityId, CancellationToken ct = default);
    }
}