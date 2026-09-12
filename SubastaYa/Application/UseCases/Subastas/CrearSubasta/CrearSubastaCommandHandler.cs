using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.CrearSubasta
{
    public class CrearSubastaCommandHandler : ICommandHandler<CrearSubastaCommand, int>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearSubastaCommandHandler(ISubastaRepository subastaRepository, IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(CrearSubastaCommand command)
        {
            if (command.PrecioBase <= 0 || command.IncrementoMinimo <= 0)
                throw new DomainException("El precio base y el incremento mínimo deben ser mayores a cero.");

            // Pasamos ambas fechas a UTC para comparar correctamente
            var fechaInicioUtc = command.FechaInicio.ToUniversalTime();
            var fechaFinUtc = command.FechaFin.ToUniversalTime();

            if (fechaFinUtc <= fechaInicioUtc)
                throw new DomainException("La fecha de finalización debe ser posterior a la fecha de inicio.");

            if (fechaFinUtc <= DateTime.UtcNow)
                throw new DomainException("La fecha de finalización debe ser futura.");

            // Si el usuario pone la fecha de ahora mismo (le damos 2 minutos de tolerancia 
            // por lo que tarde en llenar el form), arranca ACTIVA. Si no, PROGRAMADA.
            string estadoCalculado = fechaInicioUtc <= DateTime.UtcNow.AddMinutes(2) ? EstadosSubasta.Activa : EstadosSubasta.Programada;

            var nuevaSubasta = new Subasta
            {
                Titulo = command.Titulo,
                Descripcion = command.Descripcion,
                UrlImagen = command.UrlImagen,
                PrecioBase = command.PrecioBase,
                IncrementoMinimo = command.IncrementoMinimo,
                FechaInicio = fechaInicioUtc, // Se guarda la fecha elegida
                FechaFin = fechaFinUtc,
                Estado = estadoCalculado,     // Se guarda el estado calculado
                VendedorId = command.VendedorId,
                CategoriaId = command.CategoriaId,
                Version = 1
            };

            await _subastaRepository.AgregarAsync(nuevaSubasta);
            await _unitOfWork.SaveChangesAsync();

            return nuevaSubasta.Id;
        }
    }
}