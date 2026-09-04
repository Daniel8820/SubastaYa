using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SubastaRepository : ISubastaRepository
    {
        private readonly SubastaYaDbContext _context;

        public SubastaRepository(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<Subasta?> ObtenerPorIdAsync(int id)
        {
            return await _context.Subastas.FindAsync(id);
        }

        public async Task AgregarAsync(Subasta subasta)
        {
            await _context.Subastas.AddAsync(subasta);
        }

        public void Actualizar(Subasta subasta)
        {
            _context.Subastas.Update(subasta);
        }
    }
}