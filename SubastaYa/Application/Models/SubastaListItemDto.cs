namespace Application.Models
{
    public class SubastaListItemDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string UrlImagen { get; set; }
        public string Estado { get; set; }
        public decimal OfertaMasAlta { get; set; }
        public int CantidadOfertas { get; set; }
        public DateTime FechaFin { get; set; }
    }
}