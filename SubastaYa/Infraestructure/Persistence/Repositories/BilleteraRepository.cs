using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BilleteraRepository : IBilleteraRepository
    {
        private readonly SubastaYaDbContext _context;

        public BilleteraRepository(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
        }

        public void Actualizar(Billetera billetera)
        {
            _context.Billeteras.Update(billetera);
        }
    }
}