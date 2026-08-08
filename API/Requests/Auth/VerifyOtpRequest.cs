using System.ComponentModel.DataAnnotations;

namespace Requests.Auth
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "Verification code must be between 6 and 10 characters.")]
        public string Code { get; set; } = string.Empty;
    }
}
