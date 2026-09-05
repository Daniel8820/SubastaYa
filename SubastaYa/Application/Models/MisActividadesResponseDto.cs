namespace Application.Models
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
        public string Estado { get; set; }
        public decimal Recaudacion { get; set; }
        public bool Adjudicada { get; set; }
    }

    public class ParticipacionDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Estado { get; set; }
        public decimal MiOfertaMaxima { get; set; }
        public decimal OfertaGanadoraActual { get; set; }
        public bool SoyGanador { get; set; }
    }
}