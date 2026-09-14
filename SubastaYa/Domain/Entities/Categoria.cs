namespace SubastaYa.Domain.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string UrlIcono { get; set; }

        // Propiedad de navegación
        public ICollection<Subasta> Subastas { get; set; }
    }
}
