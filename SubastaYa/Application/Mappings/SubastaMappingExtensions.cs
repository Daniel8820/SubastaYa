using SubastaYa.Application.DTOs;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Mappings
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
                VendedorId = subasta.VendedorId,
                Vendedor = subasta.Vendedor != null ? subasta.Vendedor.Nombre : "Desconocido",
                Categoria = subasta.Categoria != null ? subasta.Categoria.Nombre : "Sin categoría",
                PujasTotal = subasta.Pujas.Count,
                HistorialPujas = subasta.Pujas
                    .OrderByDescending(p => p.Monto)
                    .Select(p => new PujaItemDto
                    {
                        CompradorId = p.CompradorId,
                        Monto = p.Monto,
                        Fecha = p.FechaPuja,
                        Comprador = p.Comprador != null ? p.Comprador.Nombre : "Anónimo"
                    }).ToList()
            };
        }

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
                FechaFin = subasta.FechaFin,
                FechaInicio = subasta.FechaInicio,
                Categoria = subasta.Categoria != null ? subasta.Categoria.Nombre : "Sin categoría"
            };
        }
    }
}