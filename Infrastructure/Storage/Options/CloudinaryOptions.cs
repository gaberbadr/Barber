using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Storage.Options
{
    public class CloudinaryOptions
    {
        /// <summary>
        /// Gets or sets the Cloudinary cloud name.
        /// </summary>
        public string CloudName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Cloudinary API key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Cloudinary API secret.
        /// </summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>
        /// Validates that all required settings are configured.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CloudName))
            {
                throw new InvalidOperationException("Cloudinary CloudName is not configured in appsettings.");
            }

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new InvalidOperationException("Cloudinary ApiKey is not configured in appsettings.");
            }

            if (string.IsNullOrWhiteSpace(ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary ApiSecret is not configured in appsettings.");
            }
        }
    }
}
