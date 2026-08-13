using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Authentication
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Validates that all required JWT configuration is present and valid.
        /// Called during application startup to catch configuration issues early.
        /// Throws InvalidOperationException if any required setting is missing or invalid.
        /// </summary>
        public void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Key))
            {
                errors.Add("JWT:Key is required and cannot be empty. Provide a secure key (minimum 32 characters, base64 encoded recommended).");
            }
            else if (Key.Length < 32)
            {
                errors.Add("JWT:Key is too short. Minimum 32 characters required for secure signing.");
            }

            if (string.IsNullOrWhiteSpace(Issuer))
            {
                errors.Add("JWT:Issuer is required and cannot be empty. Typically your application name or domain.");
            }

            if (string.IsNullOrWhiteSpace(Audience))
            {
                errors.Add("JWT:Audience is required and cannot be empty. Typically your application name or the consuming service name.");
            }

            if (AccessTokenExpirationMinutes <= 0)
            {
                errors.Add("JWT:AccessTokenExpirationMinutes must be greater than 0.");
            }

            if (RefreshTokenExpirationDays <= 0)
            {
                errors.Add("JWT:RefreshTokenExpirationDays must be greater than 0.");
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    $"JWT configuration is invalid:\n{string.Join("\n", errors)}\n\n" +
                    $"Please ensure the following are configured in appsettings.json under the 'JWT' section:\n" +
                    $"- Key: A secure key (minimum 32 characters, base64 encoded recommended)\n" +
                    $"- Issuer: Your application name or domain\n" +
                    $"- Audience: Your application name or consuming service name");
            }
        }
    }
}
