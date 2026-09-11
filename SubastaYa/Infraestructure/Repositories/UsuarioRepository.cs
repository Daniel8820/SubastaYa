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

        public async Task AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> ObtenerPorIdentityIdAsync(string identityId)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
        }
        public void Actualizar(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
        }
    }
}