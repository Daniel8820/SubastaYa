namespace Domain.Entities
{
    public class Billetera
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal SaldoRetenido { get; set; }
        public decimal SaldoDisponible { get; set; }

        // Campo obligatorio para Optimistic Locking según requerimientos
        public int Version { get; set; }

        // Propiedades de navegación
        public Usuario Usuario { get; set; }
    }
}
