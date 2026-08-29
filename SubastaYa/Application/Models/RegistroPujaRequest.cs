using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class RegistroPujaRequest
    {
        public int SubastaId { get; set; }
        public int CompradorId { get; set; } // En un caso real vendría del Token JWT, pero lo pedimos por parámetro para las pruebas
        public decimal Monto { get; set; }
    }
}
