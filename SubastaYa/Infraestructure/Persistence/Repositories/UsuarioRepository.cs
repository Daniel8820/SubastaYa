using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly SubastaYaDbContext _context;

        public UsuarioRepository(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public void Agregar(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
        }
    }
}