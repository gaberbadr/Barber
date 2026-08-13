using API.Helpers;
using Application.Interfaces;
using Infrastructure.Caching.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace API.Middleware
{
    /// <summary>
    /// Middleware for applying rate limiting to API endpoints.
    /// - For authenticated requests: Uses UserId as primary partition key with IP suffix
    /// - For anonymous requests: Uses a secure HttpOnly cookie-based ClientId
    /// - Applies a global IP-based limit as additional protection against bots
    /// 
    /// Designed to be resilient to rate limiting infrastructure failures.
    /// If rate limiting checks fail, requests are allowed to pass through (fail-open).
    /// 
    /// Configuration is loaded from appsettings.json under "RateLimiting" section.
    /// Configurable excluded paths and rate limit policies.
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly RateLimitingOptions _rateLimitingOptions;

        public RateLimitMiddleware(
            RequestDelegate next,
            ILogger<RateLimitMiddleware> logger,
            IOptions<RateLimitingOptions> rateLimitingOptions)
        {
            _next = next;
            _logger = logger;
            _rateLimitingOptions = rateLimitingOptions?.Value ?? new RateLimitingOptions();
        }

        /// <summary>
        /// Processes the HTTP request and applies rate limiting checks.
        /// Adds response headers and logs rejected requests with detailed information.
        /// Resilient to infrastructure failures - if rate limiting fails, request is allowed.
        /// </summary>
        public async Task InvokeAsync(
            HttpContext context,
            IRateLimitService rateLimitService,
            CurrentUser currentUser)
        {
            // Skip rate limiting for excluded endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (ShouldSkipRateLimit(path))
            {
                await _next(context);
                return;
            }

            try
            {
                // Validate that CurrentUser has the minimum data needed
                // CurrentUser can safely return null for HttpContext-dependent properties
                if (currentUser == null)
                {
                    _logger.LogWarning("CurrentUser was null in RateLimitMiddleware, allowing request to proceed");
                    await _next(context);
                    return;
                }

                // Get the partition key based on authentication status
                var partitionKey = GetPartitionKey(context, currentUser);
                if (string.IsNullOrWhiteSpace(partitionKey))
                {
                    _logger.LogWarning("Could not determine partition key for rate limiting, allowing request to proceed");
                    await _next(context);
                    return;
                }

                var globalIpKey = $"ip:{currentUser.IpAddress ?? "unknown"}";

                // Get current counts for response headers
                var globalIpCount = await rateLimitService.GetCurrentCountAsync(globalIpKey);
                var userCount = await rateLimitService.GetCurrentCountAsync(partitionKey);

                // Check global IP-based limit (bot protection)
                var globalIpAllowed = await rateLimitService.IsRequestAllowedAsync(
                    globalIpKey,
                    _rateLimitingOptions.Global.Requests,
                    _rateLimitingOptions.Global.WindowSeconds);

                if (!globalIpAllowed)
                {
                    LogRejectedRequest(
                        context,
                        currentUser,
                        "GLOBAL_RATE_LIMIT_EXCEEDED",
                        partitionKey);

                    RespondWithRateLimitExceeded(
                        context,
                        "Too many requests from your IP. Please try again later.",
                        "GLOBAL_RATE_LIMIT_EXCEEDED",
                        _rateLimitingOptions.Global.Requests,
                        _rateLimitingOptions.Global.Requests - globalIpCount,
                        _rateLimitingOptions.Global.WindowSeconds);

                    return;
                }

                // Check user/client specific rate limit
                var userLimit = currentUser.IsAuthenticated
                    ? _rateLimitingOptions.Authenticated
                    : _rateLimitingOptions.Anonymous;

                var isAllowed = await rateLimitService.IsRequestAllowedAsync(
                    partitionKey,
                    userLimit.Requests,
                    userLimit.WindowSeconds);

                if (!isAllowed)
                {
                    LogRejectedRequest(
                        context,
                        currentUser,
                        "RATE_LIMIT_EXCEEDED",
                        partitionKey);

                    RespondWithRateLimitExceeded(
                        context,
                        "Rate limit exceeded. Please try again later.",
                        "RATE_LIMIT_EXCEEDED",
                        userLimit.Requests,
                        userLimit.Requests - userCount,
                        userLimit.WindowSeconds);

                    return;
                }

                // Add rate limit response headers for allowed requests
                context.Response.Headers["X-RateLimit-Limit"] = userLimit.Requests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = (userLimit.Requests - userCount).ToString();

                // Request is allowed, proceed
                await _next(context);
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific cases like missing HttpContext or configuration issues
                _logger.LogError(
                    ex,
                    "Configuration or context error in RateLimitMiddleware for path: {Path}. " +
                    "Allowing request to proceed. Error: {ErrorMessage}",
                    context.Request.Path,
                    ex.Message);

                // Allow request to proceed (fail-open)
                await _next(context);
            }
            catch (Exception ex)
            {
                // Catch other infrastructure failures (Redis, storage, etc.)
                _logger.LogError(
                    ex,
                    "Unexpected error in RateLimitMiddleware for path: {Path}. " +
                    "Allowing request to proceed (rate limiting temporarily unavailable). Error: {ErrorMessage}",
                    context.Request.Path,
                    ex.Message);

                // Don't block the request if rate limiter fails (fail-open)
                await _next(context);
            }
        }

        /// <summary>
        /// Determines the partition key for rate limiting based on authentication status.
        /// - Authenticated: Uses UserId (primary) with IP suffix for specificity
        /// - Anonymous: Uses secure HttpOnly cookie-based ClientId
        /// </summary>
        private string GetPartitionKey(HttpContext context, CurrentUser currentUser)
        {
            // For authenticated users, use UserId as primary key
            if (currentUser.IsAuthenticated && !string.IsNullOrEmpty(currentUser.UserId))
            {
                return $"user:{currentUser.UserId}";
            }

            // For anonymous users, use a persistent ClientId cookie
            var clientId = GetOrCreateClientId(context);
            return $"client:{clientId}";
        }

        /// <summary>
        /// Gets or creates a persistent ClientId for anonymous users.
        /// Uses a secure HttpOnly cookie to identify recurring clients.
        /// Cookie is configured with:
        /// - HttpOnly: Cannot be accessed via JavaScript
        /// - Secure: Only sent over HTTPS
        /// - SameSite=Strict: CSRF protection
        /// </summary>
        private string GetOrCreateClientId(HttpContext context)
        {
            // Try to get existing ClientId cookie
            if (context.Request.Cookies.TryGetValue(_rateLimitingOptions.ClientIdCookieName, out var existingClientId))
            {
                return existingClientId;
            }

            // Generate new ClientId as GUID
            var newClientId = Guid.NewGuid().ToString();

            // Set secure HttpOnly cookie
            context.Response.Cookies.Append(
                _rateLimitingOptions.ClientIdCookieName,
                newClientId,
                new CookieOptions
                {
                    HttpOnly = true,      // Cannot be accessed via JavaScript (prevents XSS attacks)
                    Secure = true,        // Only sent over HTTPS
                    SameSite = SameSiteMode.Strict,  // CSRF protection
                    Expires = DateTimeOffset.UtcNow.AddDays(_rateLimitingOptions.ClientIdCookieExpirationDays)
                });

            _logger.LogDebug("Created new client ID cookie: {ClientId}", newClientId);

            return newClientId;
        }

        /// <summary>
        /// Determines if the current endpoint should skip rate limiting.
        /// Uses configurable excluded paths from options.
        /// </summary>
        private bool ShouldSkipRateLimit(string path)
        {
            return _rateLimitingOptions.ExcludedPaths.Any(p =>
                path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Sends a 429 Too Many Requests response with rate limit headers and body.
        /// </summary>
        private void RespondWithRateLimitExceeded(
            HttpContext context,
            string message,
            string code,
            int limit,
            int remaining,
            int windowSeconds)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, remaining).ToString();
            context.Response.Headers["Retry-After"] = windowSeconds.ToString();

            var response = new
            {
                message = message,
                code = code,
                retryAfter = windowSeconds
            };

            context.Response.WriteAsJsonAsync(response);
        }

        /// <summary>
        /// Logs a rejected request with detailed information for monitoring and debugging.
        /// Includes: UserId (if authenticated), ClientId (if anonymous), IP, endpoint, timestamp.
        /// Handles null values gracefully for safety.
        /// </summary>
        private void LogRejectedRequest(
            HttpContext context,
            CurrentUser currentUser,
            string reason,
            string partitionKey)
        {
            try
            {
                var userInfo = currentUser.IsAuthenticated
                    ? $"UserId={currentUser.UserId}"
                    : $"ClientId={partitionKey}";

                _logger.LogWarning(
                    "Rate limit rejected: {UserInfo}, IP={IpAddress}, Method={Method}, Path={Path}, Reason={Reason}, Timestamp={Timestamp}",
                    userInfo,
                    currentUser.IpAddress ?? "unknown",
                    context.Request.Method,
                    context.Request.Path,
                    reason,
                    DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                // If logging fails, don't crash - just log a generic message
                _logger.LogError(ex, "Failed to log rate limit rejection");
            }
        }
    }
}