using Microsoft.AspNetCore.Mvc;

namespace API.Filters
{
    /// <summary>
    /// Optional custom attribute for endpoint-specific rate limiting overrides.
    /// If applied to an endpoint, these limits override the global middleware defaults.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)] //[RateLimit(5, 60)] //5 Requests per 60 seconds
    public class RateLimitAttribute : Attribute
    {
        /// <summary>
        /// Gets the number of allowed requests in the time window.
        /// </summary>
        public int Requests { get; }

        /// <summary>
        /// Gets the time window in seconds.
        /// </summary>
        public int WindowSeconds { get; }

        /// Initializes a new instance of the RateLimitAttribute.
        public RateLimitAttribute(int requests, int windowSeconds = 60)
        {
            if (requests <= 0)
                throw new ArgumentException("Requests must be greater than 0", nameof(requests));

            if (windowSeconds <= 0)
                throw new ArgumentException("Window seconds must be greater than 0", nameof(windowSeconds));

            Requests = requests;
            WindowSeconds = windowSeconds;
        }
    }
}