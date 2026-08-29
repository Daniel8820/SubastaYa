using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class SubastaResponse
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string UrlImagen { get; set; }
        public decimal PrecioBase { get; set; }

        // Datos calculados para el frontend
        public decimal OfertaMasAlta { get; set; }
        public int CantidadOfertas { get; set; }

        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
    }
}
