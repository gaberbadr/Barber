using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Domain.Entities
{
    public class LoginAttempt : BaseEntity<int>
    {
        [Required]
        public string Email { get; set; }

        public DateTime AttemptedAt { get; set; }

        public bool IsSuccessful { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }
    }
}
