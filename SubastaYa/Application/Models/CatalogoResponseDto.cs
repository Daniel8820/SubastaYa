namespace Application.Models
{
    public class CatalogoResponseDto
    {
        public PaginacionInfo Paginacion { get; set; } = new();
        public List<SubastaListItemDto> Items { get; set; } = new();
    }

    public class PaginacionInfo
    {
        public int TotalItems { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
    }
}