namespace Application.UseCases.Subastas.Queries
{
    public class GetCatalogoSubastasQuery
    {
        public string Estado { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public string Orden { get; set; }
        public int Pagina { get; set; }
        public int TamañoPagina { get; set; }
    }
}