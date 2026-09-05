using Application.Models;
using Domain.Entities;

namespace Application.Mappings
{
    public static class UsuarioMappingExtensions
    {
        public static PublicacionDto ToPublicacionDto(this Subasta subasta)
        {
            return new PublicacionDto
            {
                Id = subasta.Id,
                Titulo = subasta.Titulo,
                Estado = subasta.Estado,
                Recaudacion = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : 0,
                Adjudicada = subasta.Estado == "FINALIZADA" && subasta.Pujas.Any()
            };
        }

        public static ParticipacionDto ToParticipacionDto(this Subasta subasta, int usuarioId)
        {
            return new ParticipacionDto
            {
                Id = subasta.Id,
                Titulo = subasta.Titulo,
                Estado = subasta.Estado,
                MiOfertaMaxima = subasta.Pujas.Where(p => p.CompradorId == usuarioId).Max(p => p.Monto),
                OfertaGanadoraActual = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase,
                SoyGanador = subasta.Estado == "FINALIZADA" &&
                             subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault()?.CompradorId == usuarioId
            };
        }
    }
}