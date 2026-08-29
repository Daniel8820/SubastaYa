namespace Domain.Entities
{
    public class TransaccionLedger
    {
        public int Id { get; set; }
        public int BilleteraId { get; set; }

        // Sugerencia: más adelante podrían cambiar esto por un Enum (DEPOSITO, RETENCION, LIBERACION, PAGO, COBRO)
        public string Tipo { get; set; }

        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }

        // Nullable (opcional) para trazabilidad de la subasta
        public int? SubastaId { get; set; }

        // Propiedades de navegación
        public Billetera Billetera { get; set; }
        public Subasta Subasta { get; set; }
    }
}
