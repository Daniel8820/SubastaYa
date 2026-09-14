using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Exceptions;   

namespace SubastaYa.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SubastaYaDbContext _context;

        public UnitOfWork(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyDomainException("Conflicto de concurrencia detectado en la base de datos.");
            }
            catch (DbUpdateException)
            {
                throw new DomainException("Error de integridad de datos. Verificá que las referencias enviadas (ej. CategoriaId) existan.");
            }
        }
    }
}