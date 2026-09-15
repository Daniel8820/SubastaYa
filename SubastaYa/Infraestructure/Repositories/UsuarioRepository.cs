using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly SubastaYaDbContext _context;

        public UsuarioRepository(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
        {
            await _context.Usuarios.AddAsync(usuario, ct);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Usuarios.FindAsync(new object[] { id }, ct);
        }

        public async Task<Usuario?> ObtenerPorIdentityIdAsync(string identityId, CancellationToken ct = default)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId, ct);
        }
        public void Actualizar(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
        }
    }
}