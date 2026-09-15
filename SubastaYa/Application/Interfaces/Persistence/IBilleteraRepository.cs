using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface IBilleteraRepository
    {
        Task<Billetera> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
        void Actualizar(Billetera billetera);
        void AgregarTransaccion(TransaccionLedger transaccion);
        Task<List<TransaccionLedger>> ObtenerHistorialAsync(int billeteraId, CancellationToken ct = default);
        void AgregarAuditoria(AuditoriaLog log);
    }
}