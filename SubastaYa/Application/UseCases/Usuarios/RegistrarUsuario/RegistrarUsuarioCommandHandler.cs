using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.UseCases.Usuarios.RegistrarUsuario
{
    public class RegistrarUsuarioCommandHandler : ICommandHandler<RegistrarUsuarioCommand, int>
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarUsuarioCommandHandler(
            IAuthService authService,
            IUsuarioRepository usuarioRepository,
            IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<int> HandleAsync(RegistrarUsuarioCommand command, CancellationToken ct = default)
        {
            // Delegamos a la infraestructura la creación de la clave y el token
            string nuevoIdentityId = await _authService.RegistrarLoginAsync(command.Email, command.Password);

            // Creamos la entidad en el Dominio
            var nuevoUsuario = new Usuario
            {
                Nombre = command.Nombre,
                Email = command.Email,
                FechaRegistro = DateTime.UtcNow,
                IdentityId = nuevoIdentityId,
                Billetera = new Billetera
                {
                    Version = 1
                }
            };

            // Guardamos en la tabla del negocio
            await _usuarioRepository.AgregarAsync(nuevoUsuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return nuevoUsuario.Id;
        }
    }
}