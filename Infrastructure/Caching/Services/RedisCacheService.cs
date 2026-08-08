using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Application.Interfaces;

namespace Zero.Infrastructure.Caching.Services;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer multiplexer,
        ILogger<RedisCacheService> logger)
    {
        _database = multiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        if (typeof(T) == typeof(string))
            return (T)(object)value.ToString();

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var data = value is string s
            ? s
            : JsonSerializer.Serialize(value);

        await _database.StringSetAsync(key, data, expiration);
    }

    public Task RemoveAsync(string key) => _database.KeyDeleteAsync(key);
    public Task<bool> ExistsAsync(string key) => _database.KeyExistsAsync(key);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null) return cached;
        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }
}