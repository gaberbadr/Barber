using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching.Options
{
    /// <summary>
    /// Configuration options for login rate limiting.
    /// These settings are bound from appsettings.json under "LoginRateLimiter" section.
    /// </summary>
    public class LoginRateLimiterOptions
    {
        /// <summary>
        /// Maximum number of failed login attempts allowed within the attempt window.
        /// </summary>
        public int MaxAttempts { get; set; } = 5;

        /// <summary>
        /// Time window duration in minutes for tracking failed login attempts.
        /// Only attempts within this window count toward the maximum limit.
        /// </summary>
        public int AttemptWindowMinutes { get; set; } = 15;

        /// <summary>
        /// Duration in hours for which a user is banned after exceeding max attempts.
        /// </summary>
        public int BanDurationHours { get; set; } = 3;
    }
}
