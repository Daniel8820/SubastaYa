using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Infrastructure.Repositories
{
    public class BilleteraRepository : IBilleteraRepository
    {
        private readonly SubastaYaDbContext _context;

        public BilleteraRepository(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<Billetera> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default)
        {
            return await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId, ct);
        }

        public void Actualizar(Billetera billetera)
        {
            _context.Billeteras.Update(billetera);
        }

        public void AgregarTransaccion(TransaccionLedger transaccion)
        {
            _context.TransaccionesLedger.Add(transaccion);
        }

        public async Task<List<TransaccionLedger>> ObtenerHistorialAsync(int billeteraId, CancellationToken ct = default)
        {
            return await _context.TransaccionesLedger
                .Where(t => t.BilleteraId == billeteraId)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync(ct);
        }

        public void AgregarAuditoria(AuditoriaLog log)
        {
            _context.AuditoriaLogs.Add(log);
        }
    }
}