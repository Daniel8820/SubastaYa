namespace Domain.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string UrlIcono { get; set; }

        // Propiedad de navegación: una categoría puede tener muchas subastas
        public ICollection<Subasta> Subastas { get; set; }
    }
}
