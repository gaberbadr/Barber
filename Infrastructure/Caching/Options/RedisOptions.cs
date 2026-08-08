namespace Infrastructure.Caching.Options
{
    /// <summary>
    /// Configuration options for Redis/Upstash connection.
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Gets or sets whether Redis caching is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the default expiration time in minutes for cached values.
        /// </summary>
        public int DefaultExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets the connection timeout in milliseconds.
        /// </summary>
        public int ConnectTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the sync timeout in milliseconds.
        /// </summary>
        public int SyncTimeoutMs { get; set; } = 5000;
    }
}