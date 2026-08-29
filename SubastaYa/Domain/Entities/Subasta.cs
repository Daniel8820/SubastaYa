namespace Domain.Entities
{
    public class Subasta
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public int CategoriaId { get; set; }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string UrlImagen { get; set; }

        public decimal PrecioBase { get; set; }
        public decimal IncrementoMinimo { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Puede ser un string o idealmente un Enum (PROGRAMADA, ACTIVA, FINALIZADA, DESIERTA)
        public string Estado { get; set; }

        // Campo obligatorio para Optimistic Locking
        public int Version { get; set; }

        // Propiedades de navegación
        public Usuario Vendedor { get; set; }
        public Categoria Categoria { get; set; }
        public ICollection<Puja> Pujas { get; set; }
    }
}
