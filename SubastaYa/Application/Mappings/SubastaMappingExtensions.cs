using Application.Models;
using Domain.Entities;

namespace Application.Mappings
{
    public static class SubastaMappingExtensions
    {
        public static SubastaDetalleResponseDto ToDetalleDto(this Subasta subasta)
        {
            return new SubastaDetalleResponseDto
            {
                Id = subasta.Id,
                Titulo = subasta.Titulo,
                Descripcion = subasta.Descripcion,
                UrlImagen = subasta.UrlImagen,
                PrecioBase = subasta.PrecioBase,
                IncrementoMinimo = subasta.IncrementoMinimo,
                FechaInicio = subasta.FechaInicio,
                FechaFin = subasta.FechaFin,
                Estado = subasta.Estado,
                Vendedor = subasta.Vendedor != null ? subasta.Vendedor.Nombre : "Desconocido",
                PujasTotal = subasta.Pujas.Count,
                HistorialPujas = subasta.Pujas
                    .OrderByDescending(p => p.Monto)
                    .Select(p => new PujaItemDto
                    {
                        Monto = p.Monto,
                        Fecha = p.FechaPuja,
                        Comprador = p.Comprador != null ? p.Comprador.Nombre : "Anónimo"
                    }).ToList()
            };
        }

        // 2. NUEVO: Mapper para los elementos individuales del catálogo
        public static SubastaListItemDto ToListItemDto(this Subasta subasta)
        {
            return new SubastaListItemDto
            {
                Id = subasta.Id,
                Titulo = subasta.Titulo,
                UrlImagen = subasta.UrlImagen,
                Estado = subasta.Estado,
                OfertaMasAlta = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase,
                CantidadOfertas = subasta.Pujas.Count,
                FechaFin = subasta.FechaFin
            };
        }
    }
}