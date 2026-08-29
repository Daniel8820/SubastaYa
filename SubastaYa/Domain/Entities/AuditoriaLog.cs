namespace Domain.Entities
{
    public class AuditoriaLog
    {
        public int Id { get; set; }

        // Ej: SUBASTA, BILLETERA, SISTEMA
        public string Entidad { get; set; }

        // ID del registro específico afectado
        public int EntidadId { get; set; }

        // Ej: EXTENSION_TIEMPO, CIERRE_WORKER
        public string Accion { get; set; }

        // Nullable, porque si la acción la ejecuta el Worker en segundo plano, no hay usuario
        public int? UsuarioId { get; set; }

        // Payload con los cambios en formato JSON
        public string DetalleJson { get; set; }

        public DateTime Fecha { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; }
    }
}
