namespace SubastaYa.Application.DTOs
{
    public class MisActividadesResponseDto
    {
        public List<PublicacionDto> MisPublicaciones { get; set; } = new();
        public List<ParticipacionDto> MisComprasYPujas { get; set; } = new();
    }

    public class PublicacionDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string UrlImagen { get; set; }
        public string Estado { get; set; }
        public decimal Recaudacion { get; set; }
        public bool Adjudicada { get; set; }

        // Para ver el tiempo de la publicacion 
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class ParticipacionDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string UrlImagen { get; set; }
        public string Estado { get; set; }
        public decimal MiOfertaMaxima { get; set; }
        public decimal OfertaGanadoraActual { get; set; }
        public bool SoyGanador { get; set; }

        // Para ver el tiempo de las publicaciones
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}