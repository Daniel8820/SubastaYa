namespace Application.Models
{
    public class SubastaDetalleResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string UrlImagen { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal IncrementoMinimo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string Vendedor { get; set; }
        public int PujasTotal { get; set; }
        public List<PujaItemDto> HistorialPujas { get; set; } = new();
    }

    public class PujaItemDto
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Comprador { get; set; }
    }
}