namespace Application.Features.Auth.DTOs
{
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenResponseDTO? Token { get; set; }
        public int? BanMinutes { get; set; }
    }
}