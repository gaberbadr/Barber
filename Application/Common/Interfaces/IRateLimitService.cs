using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    /// <summary>
    /// Defines rate limiting logic for protecting endpoints.
    /// Supports both authenticated (by UserId) and anonymous (by ClientId/Cookie) rate limiting.
    public interface IRateLimitService
    {
        /// Checks if a request is allowed based on the rate limit policy.
        Task<bool> IsRequestAllowedAsync(
            string partitionKey,
            int allowedRequests,
            int windowSeconds);


        /// Resets the rate limit counter for a specific partition key (e.g., after logout or ban lift).
        Task ResetAsync(string partitionKey);

        /// Gets the current request count for a partition key (useful for debugging/monitoring).
        Task<int> GetCurrentCountAsync(string partitionKey);
    }
}


/// <summary>
/// Defines distributed rate limiting for protecting endpoints.
/// 
/// Storage Strategy:
/// - Primary: Upstash Redis via StackExchange.Redis (IConnectionMultiplexer singleton)
///   Uses atomic INCR command with automatic expiration per time window.
/// - Fallback: ConcurrentDictionary (in-memory, per-server, when Redis unavailable)
///   Auto-recovery: switches back to Redis when connection restored (no restart needed).
/// 
/// User Identification:
/// - Authenticated: "user:{userId}" partition key (from JWT claims)
/// - Anonymous: "client:{clientId}" partition key (secure HttpOnly cookie GUID, 30-day expiry)
/// - Global Protection: "ip:{ipAddress}" (bot protection, applied to all requests)
/// 
/// Partition Key Format: "ratelimit:{partitionKey}" in Redis
/// 
/// Behavior:
/// - Fail-open: Request continues even if rate limiter errors
/// - Atomic: Redis INCR ensures thread-safe distributed counting
/// - TTL: Keys auto-expire after windowSeconds (Redis only, manual cleanup for in-memory)
/// </summary>