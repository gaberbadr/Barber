namespace Infrastructure.Caching.Options
{
    /// <summary>
    /// Configuration options for rate limiting policies.
    /// These settings are bound from appsettings.json under "RateLimiting" section.
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Configuration for authenticated user rate limiting.
        /// </summary>
        public RateLimitPolicy Authenticated { get; set; } = new();

        /// <summary>
        /// Configuration for anonymous user rate limiting.
        /// </summary>
        public RateLimitPolicy Anonymous { get; set; } = new();

        /// <summary>
        /// Global IP-based rate limiting (bot protection).
        /// </summary>
        public RateLimitPolicy Global { get; set; } = new();

        /// <summary>
        /// List of URL paths that should be excluded from rate limiting.
        /// </summary>
        public string[] ExcludedPaths { get; set; } = new[]
        {
            "/health",
            "/swagger",
            "/.well-known",
            "/metrics"
        };

        /// <summary>
        /// Name of the secure cookie used to identify anonymous clients.
        /// </summary>
        public string ClientIdCookieName { get; set; } = "X-Client-Id";

        /// <summary>
        /// Expiration time in days for the client ID cookie.
        /// </summary>
        public int ClientIdCookieExpirationDays { get; set; } = 30;
    }

    /// <summary>
    /// Represents a rate limiting policy with request limit and time window.
    /// </summary>
    public class RateLimitPolicy
    {
        /// <summary>
        /// Maximum number of requests allowed within the time window.
        /// </summary>
        public int Requests { get; set; } = 100;

        /// <summary>
        /// Time window duration in seconds.
        /// </summary>
        public int WindowSeconds { get; set; } = 60;
    }
}