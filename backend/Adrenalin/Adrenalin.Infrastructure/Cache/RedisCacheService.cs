using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Cache;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedBytes = await _cache.GetAsync(key, cancellationToken);
            if (cachedBytes == null)
            {
                _logger.LogInformation("Cache MISS: {CacheKey}", key);
                return default;
            }

            _logger.LogInformation("Cache HIT: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cachedBytes, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cache for key: {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            await _cache.SetAsync(key, bytes, options, cancellationToken);
            _logger.LogInformation("Cache WRITE: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache for key: {CacheKey}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }
        
        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Cache INVALIDATED: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache for key: {CacheKey}", key);
        }
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // StackExchange.Redis doesn't have a direct "RemoveByPrefix" in IDistributedCache.
        // We will just log a warning for now. In a fully fleshed out Redis implementation, 
        // one would use IConnectionMultiplexer to run KEYS or SCAN to delete prefix keys.
        _logger.LogWarning("RemoveByPrefixAsync not natively supported by IDistributedCache for prefix {Prefix}", prefix);
        return Task.CompletedTask;
    }
}
