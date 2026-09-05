using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBilleteraRepository
    {
        Task<Billetera> ObtenerPorUsuarioIdAsync(int usuarioId);
        void Actualizar(Billetera billetera);

        // NUEVOS:
        void AgregarTransaccion(TransaccionLedger transaccion);
        Task<List<TransaccionLedger>> ObtenerHistorialAsync(int billeteraId);
        void AgregarAuditoria(AuditoriaLog log);
    }
}