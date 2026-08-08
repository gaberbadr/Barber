using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(10)]
        public string? VerificationCode { get; set; }

        public DateTime? CodeExpiresAt { get; set; }

        // Navigation Properties
        public ICollection<RefreshTokenTable> RefreshTokens { get; set; } = new List<RefreshTokenTable>();

        public ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    }
}
