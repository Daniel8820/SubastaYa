namespace SubastaYa.Application.UseCases.Usuarios.ActualizarPerfil
{
    public class ActualizarPerfilCommand
    {
        // No pedimos el ID en el body porque lo sacamos del Token por seguridad
        public int UsuarioId { get; set; }
        public string NuevoNombre { get; set; }
    }
}
