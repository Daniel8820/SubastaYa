using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Domain.Entities
{
    public class Subasta
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public int CategoriaId { get; set; }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string UrlImagen { get; set; }

        public decimal PrecioBase { get; set; }
        public decimal IncrementoMinimo { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Puede ser un string o idealmente un Enum (PROGRAMADA, ACTIVA, FINALIZADA, DESIERTA)
        public string Estado { get; set; }

        // Campo obligatorio para Optimistic Locking
        public int Version { get; set; }

        // Propiedades de navegación
        public Usuario Vendedor { get; set; }
        public Categoria Categoria { get; set; }
        public ICollection<Puja> Pujas { get; set; }

        public bool ProcesarPuja(Puja nuevaPuja)
        {
            if (Estado != EstadosSubasta.Activa || FechaFin <= DateTime.UtcNow)
                throw new DomainException("La subasta ya ha finalizado o no se encuentra activa.");

            if (VendedorId == nuevaPuja.CompradorId)
                throw new DomainException("No puedes pujar en una subasta que tú mismo has publicado.");

            var pujaGanadoraActual = Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
            if (pujaGanadoraActual != null && pujaGanadoraActual.CompradorId == nuevaPuja.CompradorId)
                throw new DomainException("Ya posees la oferta más alta en esta subasta. No puedes pujar contra ti mismo.");

            bool esPrimeraPuja = !Pujas.Any();
            var ofertaMasAlta = esPrimeraPuja ? PrecioBase : Pujas.Max(p => p.Monto);
            var montoMinimoRequerido = esPrimeraPuja && nuevaPuja.Monto == PrecioBase
                ? PrecioBase
                : ofertaMasAlta + IncrementoMinimo;

            if (nuevaPuja.Monto < montoMinimoRequerido)
            {
                if (esPrimeraPuja)
                    throw new DomainException($"La primera oferta debe ser exactamente el Precio Base (${PrecioBase}) o un mínimo de ${PrecioBase + IncrementoMinimo}.");
                else
                    throw new DomainException($"El monto es inválido. Debe superar la oferta actual por al menos ${IncrementoMinimo} (Mínimo: ${montoMinimoRequerido}).");
            }

            bool tiempoExtendido = false;
            var tiempoRestante = FechaFin - DateTime.UtcNow;
            if (tiempoRestante.TotalSeconds > 0 && tiempoRestante.TotalSeconds <= 60)
            {
                FechaFin = FechaFin.AddMinutes(2);
                tiempoExtendido = true;
            }

            Pujas.Add(nuevaPuja);
            Version++; // Avanzamos el token de concurrencia

            return tiempoExtendido;
        }
    }
}
