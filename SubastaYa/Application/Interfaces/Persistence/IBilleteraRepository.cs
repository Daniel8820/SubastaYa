using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence
{
    public interface IBilleteraRepository
    {
        Task<Billetera> ObtenerPorUsuarioIdAsync(int usuarioId);
        void Actualizar(Billetera billetera);
        void AgregarTransaccion(TransaccionLedger transaccion);
        Task<List<TransaccionLedger>> ObtenerHistorialAsync(int billeteraId);
        void AgregarAuditoria(AuditoriaLog log);
    }
}