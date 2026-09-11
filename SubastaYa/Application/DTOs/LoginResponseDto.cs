namespace SubastaYa.Application.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}