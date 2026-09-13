using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SubastaYa.Application.Interfaces.Services;

namespace SubastaYa.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UsuarioId
        {
            get
            {
                var idString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                return string.IsNullOrEmpty(idString) ? 0 : int.Parse(idString);
            }
        }

        public string Nombre => _httpContextAccessor.HttpContext?.User?.FindFirst("nombre")?.Value ?? "Anónimo";
    }
}