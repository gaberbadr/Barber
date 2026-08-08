using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Caching.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Caching.Services
{
    /// <summary>
    /// Login rate limiting service implementation using Redis Upstash if available, with in-memory fallback.
    /// Enforces maximum failed login attempts within a configurable time window, with configurable ban duration.
    /// Reuses the existing IConnectionMultiplexer singleton from infrastructure layer.
    /// </summary>
    internal class LoginRateLimiterService : ILoginRateLimiterService
    {
        private readonly IConnectionMultiplexer? _redisConnection;
        private readonly LoginRateLimiterOptions _options;
        private readonly ILogger<LoginRateLimiterService> _logger;

        // In-memory fallback: ConcurrentDictionary<email, (attemptTimestamps, banExpiryTime)>
        private readonly ConcurrentDictionary<string, (List<DateTime> Attempts, DateTime BanExpiryTime)> _inMemoryStore;

        // Redis key prefixes for rate limiting
        private const string AttemptsKeyPrefix = "login:attempts:";
        private const string BanKeyPrefix = "login:ban:";

        private readonly TimeSpan _banDuration;
        private readonly TimeSpan _attemptWindow;

        public LoginRateLimiterService(
            IConnectionMultiplexer? redisConnection,
            IOptions<LoginRateLimiterOptions> options,
            ILogger<LoginRateLimiterService> logger)
        {
            _redisConnection = redisConnection;
            _options = options?.Value ?? new LoginRateLimiterOptions();
            _logger = logger;
            _inMemoryStore = new ConcurrentDictionary<string, (List<DateTime>, DateTime)>();

            // Initialize timespan values from options
            _banDuration = TimeSpan.FromHours(_options.BanDurationHours);
            _attemptWindow = TimeSpan.FromMinutes(_options.AttemptWindowMinutes);
        }

        /// <summary>
        /// Checks if a login attempt is allowed for the given email.
        /// Returns false if user has exceeded max attempts and ban is still active.
        /// Uses Redis for distributed rate limiting, falls back to in-memory storage.
        /// </summary>
        public async Task<(bool IsAllowed, TimeSpan? BanDuration)> CheckLoginAttemptAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var attemptsKey = $"{AttemptsKeyPrefix}{email}";
            var banKey = $"{BanKeyPrefix}{email}";

            // Try Redis first if available
            if (_redisConnection?.IsConnected == true)
            {
                try
                {
                    return await CheckRedisLoginAttemptAsync(attemptsKey, banKey, email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Redis login rate limit check failed for email: {Email}. Falling back to in-memory storage.",
                        email);

                    // Fallback to in-memory
                    return CheckInMemoryLoginAttempt(email);
                }
            }

            // Use in-memory storage as fallback
            return CheckInMemoryLoginAttempt(email);
        }

        /// <summary>
        /// Records a login attempt for the given email.
        /// If the attempt is unsuccessful, increments the attempt counter.
        /// If max attempts are exceeded, sets a ban expiry time.
        /// </summary>
        public async Task RecordLoginAttemptAsync(string email, bool isSuccessful, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address cannot be null or empty", nameof(ipAddress));

            var attemptsKey = $"{AttemptsKeyPrefix}{email}";
            var banKey = $"{BanKeyPrefix}{email}";

            // If successful, reset attempts
            if (isSuccessful)
            {
                await ResetLoginAttemptsAsync(email);
                return;
            }

            // Try Redis first if available
            if (_redisConnection?.IsConnected == true)
            {
                try
                {
                    await RecordRedisLoginAttemptAsync(attemptsKey, banKey, email);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to record Redis login attempt for email: {Email}. Falling back to in-memory.",
                        email);
                }
            }

            // In-memory recording
            RecordInMemoryLoginAttempt(email);
        }

        /// <summary>
        /// Resets all login attempts for the given email.
        /// Also removes any active ban.
        /// </summary>
        public async Task ResetLoginAttemptsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var attemptsKey = $"{AttemptsKeyPrefix}{email}";
            var banKey = $"{BanKeyPrefix}{email}";

            // Try Redis first if available
            if (_redisConnection?.IsConnected == true)
            {
                try
                {
                    var db = _redisConnection.GetDatabase();
                    await db.KeyDeleteAsync(new[] { (RedisKey)attemptsKey, (RedisKey)banKey });
                    _logger.LogDebug("Login attempts and ban reset in Redis for email: {Email}", email);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to reset login attempts in Redis for email: {Email}. Falling back to in-memory.",
                        email);
                }
            }

            // In-memory reset
            if (_inMemoryStore.TryRemove(email, out _))
            {
                _logger.LogDebug("Login attempts and ban reset in-memory for email: {Email}", email);
            }
        }

        /// <summary>
        /// Checks login attempt using Redis.
        /// Stores failed attempts as timestamps in a Redis list with expiration.
        /// Uses a separate key for ban tracking.
        /// </summary>
        private async Task<(bool IsAllowed, TimeSpan? BanDuration)> CheckRedisLoginAttemptAsync(
            string attemptsKey,
            string banKey,
            string email)
        {
            var db = _redisConnection!.GetDatabase();
            var now = DateTime.UtcNow;
            var cutoffTime = now.Subtract(_attemptWindow);

            // Check if currently banned
            var banExpiry = await db.StringGetAsync(banKey);
            if (banExpiry.HasValue && DateTime.TryParse(banExpiry.ToString(), out var parsedBanExpiryTime))
            {
                var remainingBanTime = parsedBanExpiryTime - now;
                if (remainingBanTime > TimeSpan.Zero)
                {
                    _logger.LogDebug(
                        "Email {Email} is currently banned. Remaining ban time: {RemainingBanTime}",
                        email,
                        remainingBanTime);
                    return (false, remainingBanTime);
                }
                else
                {
                    // Ban has expired, remove it
                    await db.KeyDeleteAsync(banKey);
                }
            }

            // Get recent failed attempts within the window
            var allAttempts = await db.ListRangeAsync(attemptsKey);
            var recentAttempts = allAttempts
                .Where(x => x.HasValue)
                .Select(x => DateTime.Parse(x.ToString()))
                .Where(x => x > cutoffTime)
                .ToList();

            if (recentAttempts.Count >= _options.MaxAttempts)
            {
                // User exceeded max attempts - set ban
                var newBanExpiryTime = now.Add(_banDuration);
                await db.StringSetAsync(banKey, newBanExpiryTime.ToString("O"), _banDuration);
                _logger.LogWarning(
                    "Email {Email} exceeded max login attempts. Banned until {BanExpiryTime}",
                    email,
                    newBanExpiryTime);
                return (false, _banDuration);
            }

            _logger.LogDebug(
                "Redis login check: Email={Email}, RecentAttempts={RecentAttempts}, MaxAttempts={MaxAttempts}, IsAllowed=true",
                email,
                recentAttempts.Count,
                _options.MaxAttempts);

            return (true, null);
        }

        /// <summary>
        /// Records a failed login attempt in Redis.
        /// Adds timestamp to list and sets expiration window.
        /// </summary>
        private async Task RecordRedisLoginAttemptAsync(string attemptsKey, string banKey, string email)
        {
            var db = _redisConnection!.GetDatabase();
            var timestamp = DateTime.UtcNow.ToString("O");

            // Add attempt timestamp to list
            await db.ListRightPushAsync(attemptsKey, timestamp);

            // Set expiration on attempts key (15 minutes)
            await db.KeyExpireAsync(attemptsKey, _attemptWindow);

            _logger.LogDebug("Failed login attempt recorded in Redis for email: {Email}", email);
        }

        /// <summary>
        /// Checks login attempt using in-memory storage.
        /// Thread-safe using ConcurrentDictionary with timestamp tracking and ban expiry.
        /// </summary>
        private (bool IsAllowed, TimeSpan? BanDuration) CheckInMemoryLoginAttempt(string email)
        {
            var now = DateTime.UtcNow;

            if (_inMemoryStore.TryGetValue(email, out var entry))
            {
                // Check if ban has expired
                if (now < entry.BanExpiryTime)
                {
                    var remainingBanTime = entry.BanExpiryTime - now;
                    _logger.LogDebug(
                        "Email {Email} is banned in-memory. Remaining ban time: {RemainingBanTime}",
                        email,
                        remainingBanTime);
                    return (false, remainingBanTime);
                }

                // Clean up expired attempts
                var cutoffTime = now.Subtract(_attemptWindow);
                var recentAttempts = entry.Attempts
                    .Where(x => x > cutoffTime)
                    .ToList();

                if (recentAttempts.Count >= _options.MaxAttempts)
                {
                    // User exceeded max attempts - set ban
                    var newBanExpiryTime = now.Add(_banDuration);
                    _inMemoryStore[email] = (recentAttempts, newBanExpiryTime);
                    _logger.LogWarning(
                        "Email {Email} exceeded max login attempts in-memory. Banned until {BanExpiryTime}",
                        email,
                        newBanExpiryTime);
                    return (false, _banDuration);
                }

                _logger.LogDebug(
                    "In-memory login check: Email={Email}, RecentAttempts={RecentAttempts}, MaxAttempts={MaxAttempts}, IsAllowed=true",
                    email,
                    recentAttempts.Count,
                    _options.MaxAttempts);

                return (true, null);
            }

            return (true, null);
        }

        /// <summary>
        /// Records a failed login attempt in in-memory storage.
        /// Adds current timestamp to attempts list for the email.
        /// </summary>
        private void RecordInMemoryLoginAttempt(string email)
        {
            var now = DateTime.UtcNow;

            _inMemoryStore.AddOrUpdate(
                email,
                (new List<DateTime> { now }, DateTime.MinValue),  // New entry
                (key, oldValue) =>
                {
                    var cutoffTime = now.Subtract(_attemptWindow);
                    var recentAttempts = oldValue.Attempts
                        .Where(x => x > cutoffTime)
                        .ToList();

                    recentAttempts.Add(now);

                    // Check if ban should be applied
                    var newBanExpiryTime = recentAttempts.Count >= _options.MaxAttempts
                        ? now.Add(_banDuration)
                        : DateTime.MinValue;

                    return (recentAttempts, newBanExpiryTime);
                });

            _logger.LogDebug("Failed login attempt recorded in-memory for email: {Email}", email);
        }
    }
}
