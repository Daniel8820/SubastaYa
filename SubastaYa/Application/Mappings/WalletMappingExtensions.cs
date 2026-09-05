using Application.Models;
using Domain.Entities;

namespace Application.Mappings
{
    public static class WalletMappingExtensions
    {
        public static TransaccionDto ToDto(this TransaccionLedger transaccion)
        {
            return new TransaccionDto
            {
                Id = transaccion.Id,
                Tipo = transaccion.Tipo,
                Monto = transaccion.Monto,
                Fecha = transaccion.Fecha,
                SubastaId = transaccion.SubastaId
            };
        }
    }
}