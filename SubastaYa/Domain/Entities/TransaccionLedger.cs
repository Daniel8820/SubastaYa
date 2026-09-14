namespace SubastaYa.Domain.Entities
{
    public class TransaccionLedger
    {
        public int Id { get; set; }
        public int BilleteraId { get; set; }
        public string Tipo { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        // Nullable para trazabilidad de la subasta
        public int? SubastaId { get; set; }
        // Propiedades de navegación
        public Billetera Billetera { get; set; }
        public Subasta Subasta { get; set; }
    }
}
