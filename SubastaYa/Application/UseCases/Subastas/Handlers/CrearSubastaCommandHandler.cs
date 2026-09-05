using Application.Interfaces;
using Application.UseCases.Subastas.Commands;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Subastas.Handlers
{
    public class CrearSubastaCommandHandler
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
            {
                throw new DomainException("El precio base y el incremento mínimo deben ser mayores a cero.");
            }

            if (command.FechaFin.ToUniversalTime() <= DateTime.UtcNow)
            {
                throw new DomainException("La fecha de finalización debe ser futura.");
            }

            var nuevaSubasta = new Subasta
            {
                Titulo = command.Titulo,
                Descripcion = command.Descripcion,
                UrlImagen = command.UrlImagen,
                PrecioBase = command.PrecioBase,
                IncrementoMinimo = command.IncrementoMinimo,
                FechaInicio = DateTime.UtcNow,
                FechaFin = command.FechaFin.ToUniversalTime(),
                Estado = "ACTIVA",
                VendedorId = command.VendedorId,
                CategoriaId = command.CategoriaId
            };

            await _subastaRepository.AgregarAsync(nuevaSubasta);
            await _unitOfWork.SaveChangesAsync();

            return nuevaSubasta.Id;
        }
    }
}