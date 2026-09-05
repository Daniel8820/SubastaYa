namespace Application.Models
{
    public class SaldoResponseDto
    {
        public decimal Total { get; set; }
        public decimal Retenido { get; set; }
        public decimal Disponible { get; set; }
    }

    public class TransaccionDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public int? SubastaId { get; set; }
    }
}