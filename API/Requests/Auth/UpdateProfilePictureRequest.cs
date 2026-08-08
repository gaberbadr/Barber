using System.ComponentModel.DataAnnotations;

namespace Requests.Auth
{
    public class UpdateProfilePictureRequest
    {
        [Required(ErrorMessage = "Profile picture file is required.")]
        public IFormFile File { get; set; } = null!;
    }
}