namespace Application.Models
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}