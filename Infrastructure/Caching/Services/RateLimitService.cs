using System;
using System.Collections.Concurrent;
using Application.Interfaces;
using Infrastructure.Caching.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;


namespace Zero.Infrastructure.Services
{
    /// <summary>
    /// Rate limiting service implementation using Redis Upstash if available, with in-memory fallback.
    /// Tracks request counts per partition key within a configurable time window.
    /// Reuses the existing IConnectionMultiplexer singleton from infrastructure layer.
    /// </summary>
    public class RateLimitService : IRateLimitService
    {
        private readonly IConnectionMultiplexer? _redisConnection;
        private readonly RedisOptions _redisOptions;
        private readonly ILogger<RateLimitService> _logger;

        // In-memory fallback: ConcurrentDictionary<key, (count, expiryTime)>
        private readonly ConcurrentDictionary<string, (int Count, DateTime ExpiryTime)> _inMemoryStore;

        // Redis key prefix for all rate limit keys
        private const string KeyPrefix = "ratelimit:";

        public RateLimitService(
            IConnectionMultiplexer? redisConnection,
            IOptions<RedisOptions> redisOptions,
            ILogger<RateLimitService> logger)
        {
            _redisConnection = redisConnection;
            _redisOptions = redisOptions?.Value ?? new RedisOptions();
            _logger = logger;
            _inMemoryStore = new ConcurrentDictionary<string, (int, DateTime)>();
        }

        /// <summary>
        /// Checks if a request is allowed based on the rate limit policy.
        /// Uses Redis if available for distributed rate limiting across multiple instances.
        /// Falls back to in-memory storage for single-instance deployments.
        /// </summary>
        public async Task<bool> IsRequestAllowedAsync(
            string partitionKey,
            int allowedRequests,
            int windowSeconds)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentException("Partition key cannot be null or empty", nameof(partitionKey));

            if (allowedRequests <= 0)
                throw new ArgumentException("Allowed requests must be greater than 0", nameof(allowedRequests));

            if (windowSeconds <= 0)
                throw new ArgumentException("Window seconds must be greater than 0", nameof(windowSeconds));

            var redisKey = $"{KeyPrefix}{partitionKey}";

            // Try Redis first if available
            if (_redisConnection?.IsConnected == true)
            {
                try
                {
                    return await CheckRedisRateLimitAsync(redisKey, allowedRequests, windowSeconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Redis rate limit check failed for key: {RedisKey}. Falling back to in-memory storage.",
                        redisKey);

                    // Fallback to in-memory if Redis fails
                    return CheckInMemoryRateLimit(partitionKey, allowedRequests, windowSeconds);
                }
            }

            // Use in-memory storage as fallback (Redis not connected)
            return CheckInMemoryRateLimit(partitionKey, allowedRequests, windowSeconds);
        }

        /// <summary>
        /// Resets the rate limit counter for a partition key.
        /// </summary>
        public async Task ResetAsync(string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentException("Partition key cannot be null or empty", nameof(partitionKey));

            var redisKey = $"{KeyPrefix}{partitionKey}";

            // Try Redis first if available
            if (_redisConnection?.IsConnected == true)
            {
                try
                {
                    var db = _redisConnection.GetDatabase();
                    await db.KeyDeleteAsync(redisKey);
                    _logger.LogDebug("Rate limit counter reset for key: {RedisKey}", redisKey);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to reset rate limit counter in Redis for key: {RedisKey}. Falling back to in-memory.",
                        redisKey);
                }
            }

            // In-memory reset
            if (_inMemoryStore.TryRemove(partitionKey, out _))
            {
                _logger.LogDebug("Rate limit counter reset in-memory for key: {PartitionKey}", partitionKey);
            }
        }

        /// <summary>
        /// Gets the current request count for a partition key.
        /// </summary>
        public async Task<int> GetCurrentCountAsync(string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                return 0;

            var redisKey = $"{KeyPrefix}{partitionKey}";

            // Try Redis first if available
            if (_redisConnection?.IsConnected == true)
            {
                try
                {
                    var db = _redisConnection.GetDatabase();
                    var value = await db.StringGetAsync(redisKey);
                    return value.HasValue ? (int)value : 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to get rate limit count from Redis for key: {RedisKey}. Falling back to in-memory.",
                        redisKey);
                }
            }

            // In-memory retrieval
            if (_inMemoryStore.TryGetValue(partitionKey, out var entry))
            {
                // Check if expired
                if (DateTime.UtcNow > entry.ExpiryTime)
                {
                    _inMemoryStore.TryRemove(partitionKey, out _);
                    return 0;
                }

                return entry.Count;
            }

            return 0;
        }

        /// <summary>
        /// Checks rate limit using Redis.
        /// Uses Redis INCR command with expiration to atomically increment counter.
        /// INCR is atomic and thread-safe across distributed instances.
        /// </summary>
        private async Task<bool> CheckRedisRateLimitAsync(
            string redisKey,
            int allowedRequests,
            int windowSeconds)
        {
            var db = _redisConnection!.GetDatabase();

            // Atomically increment counter and get new value
            // INCR operation is atomic in Redis
            var count = await db.StringIncrementAsync(redisKey);

            // If this is the first request, set expiration
            // This is safe because INCR returns the new value
            if (count == 1)
            {
                await db.KeyExpireAsync(redisKey, TimeSpan.FromSeconds(windowSeconds));
            }

            var isAllowed = count <= allowedRequests;

            _logger.LogDebug(
                "Redis rate limit check: Key={RedisKey}, Count={Count}, Allowed={AllowedRequests}, IsAllowed={IsAllowed}",
                redisKey,
                count,
                allowedRequests,
                isAllowed);

            return isAllowed;
        }

        /// <summary>
        /// Checks rate limit using in-memory storage.
        /// Thread-safe using ConcurrentDictionary with expiry time tracking.
        /// Used as fallback when Redis is unavailable.
        /// </summary>
        private bool CheckInMemoryRateLimit(
            string partitionKey,
            int allowedRequests,
            int windowSeconds)
        {
            var now = DateTime.UtcNow;
            var expiryTime = now.AddSeconds(windowSeconds);

            // Update or insert entry
            var result = _inMemoryStore.AddOrUpdate(
                partitionKey,
                (1, expiryTime),  // New entry starts at 1
                (key, oldValue) =>
                {
                    // If expired, reset counter
                    if (now > oldValue.ExpiryTime)
                    {
                        return (1, expiryTime);
                    }

                    // Increment counter if still valid
                    var newCount = oldValue.Count + 1;
                    return (newCount, oldValue.ExpiryTime);
                });

            // Clean up expired entries periodically
            CleanExpiredInMemoryEntries();

            var isAllowed = result.Count <= allowedRequests;

            _logger.LogDebug(
                "In-memory rate limit check: Key={PartitionKey}, Count={Count}, Allowed={AllowedRequests}, IsAllowed={IsAllowed}",
                partitionKey,
                result.Count,
                allowedRequests,
                isAllowed);

            return isAllowed;
        }

        /// <summary>
        /// Cleans up expired entries from in-memory store to prevent unbounded memory growth.
        /// Runs periodically (every request, but only processes small batch).
        /// Only removes a few entries to avoid performance impact.
        /// </summary>
        private void CleanExpiredInMemoryEntries()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _inMemoryStore
                .Where(kvp => now > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .Take(100)  // Limit cleanup to avoid performance impact
                .ToList();

            if (expiredKeys.Count > 0)
            {
                foreach (var key in expiredKeys)
                {
                    _inMemoryStore.TryRemove(key, out _);
                }

                _logger.LogDebug("Cleaned up {CleanedCount} expired in-memory rate limit entries", expiredKeys.Count);
            }
        }
    }
}