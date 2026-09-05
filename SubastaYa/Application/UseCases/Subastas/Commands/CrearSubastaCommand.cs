namespace Application.UseCases.Subastas.Commands
{
    public class CrearSubastaCommand
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string UrlImagen { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal IncrementoMinimo { get; set; }
        public DateTime FechaFin { get; set; }
        public int VendedorId { get; set; }
        public int CategoriaId { get; set; }
    }
}