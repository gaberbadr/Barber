using System.ComponentModel.DataAnnotations;

namespace Requests.Auth
{
    public class RevokeRefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
