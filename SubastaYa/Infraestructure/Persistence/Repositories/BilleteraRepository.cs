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

        public async Task<Billetera> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
        }

        public void Actualizar(Billetera billetera)
        {
            _context.Billeteras.Update(billetera);
        }

        public void AgregarTransaccion(TransaccionLedger transaccion)
        {
            _context.TransaccionesLedger.Add(transaccion);
        }

        public async Task<List<TransaccionLedger>> ObtenerHistorialAsync(int billeteraId)
        {
            return await _context.TransaccionesLedger
                .Where(t => t.BilleteraId == billeteraId)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
        }

        public void AgregarAuditoria(AuditoriaLog log)
        {
            _context.AuditoriaLogs.Add(log);
        }
    }
}