using Application.Interfaces.Services;
using Application.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class SubastaService : ISubastaService
    {
        private readonly SubastaYaDbContext _context;

        public SubastaService(SubastaYaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarPujaAsync(RegistroPujaRequest request)
        {
            // Iniciamos la transacción atómica (ACID). Si algo falla, se hace rollback automático.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // TODO 1: Traer la subasta de la BD (incluyendo su Billetera si fuera necesario)
                // y verificar que el Estado sea "ACTIVA".

                // TODO 2: Validar que el monto de la nueva puja supere a la oferta actual
                // por al menos el IncrementoMinimo definido en la subasta.

                // TODO 3: Traer la billetera del Comprador y verificar que el SaldoDisponible >= Monto.

                // TODO 4: LÓGICA DE ESCROW (Garantía)
                // - A la billetera del NUEVO comprador: restarle el SaldoDisponible y sumarle al SaldoRetenido.
                // - A la billetera del comprador ANTERIOR (si existe): restarle el SaldoRetenido y sumarlo al SaldoDisponible.

                // TODO 5: REGLA ANTI-SNIPING
                // Si (FechaFin - FechaActual) < 1 minuto -> Extender FechaFin por 2 minutos.

                // TODO 6: Generar los registros en el Ledger (TransaccionLedger)
                // para auditar los movimientos de dinero retenido/liberado.

                // TODO 7: Crear el objeto Puja y agregarlo al _context.

                // Guardamos los cambios. Aquí EF Core validará el Optimistic Locking (el campo Version).
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si EF Core detecta que alguien más modificó la subasta o la billetera en el mismo instante
                await transaction.RollbackAsync();
                throw new Exception("Conflicto de concurrencia: la subasta fue modificada por otro usuario.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Aquí podrían loguear el error o usar el AuditoriaLog
                throw;
            }
        }
    }
}
