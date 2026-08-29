using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface ISubastaService
    {
        // Retornamos un booleano o podríamos retornar un objeto Result con el detalle
        Task<bool> RegistrarPujaAsync(RegistroPujaRequest request);
    }
}
