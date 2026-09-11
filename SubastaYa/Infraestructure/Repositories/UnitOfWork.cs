using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SubastaYaDbContext _context;

        public UnitOfWork(SubastaYaDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }
    }
}