using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;

namespace VendorHub.Services.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public CacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<(bool Found, T? Value)> TryGetAsync<T>(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogInformation("Cache HIT (L1): {Key}", key);
                return (true, cachedValue);
            }

            var redisData = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(redisData))
            {
                try
                {
                    var value = JsonSerializer.Deserialize<T>(redisData, jsonOptions);
                    var ttl = expiration ?? TimeSpan.FromMinutes(10);
                    _memoryCache.Set(key, value, ttl);
                    _logger.LogInformation("Cache HIT (L2): {Key}", key);
                    return (true, value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached value for key: {Key}", key);
                }
            }

            _logger.LogInformation("Cache MISS: {Key}", key);
            return (false, default);
        }

        public async Task<T?> GetAsync<T>(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var (found, value) = await TryGetAsync<T>(key, expiration, cancellationToken);
            return found ? value : default;
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            TimeSpan ttl = expiration ?? TimeSpan.FromMinutes(10);

            var (found, cached) = await TryGetAsync<T>(key, ttl, cancellationToken);
            if (found && cached != null)
                return cached;

            _logger.LogInformation("Executing factory for: {Key}", key);
            var value = await factory();

            if (value != null)
                await SetAsync(key, value, ttl, cancellationToken);

            return value;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (value == null) return;

            var ttl = expiration ?? TimeSpan.FromMinutes(10);

            _memoryCache.Set(key, value, ttl);

            var cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            string json = JsonSerializer.Serialize(value, jsonOptions);
            await _distributedCache.SetStringAsync(key, json, cacheOptions, cancellationToken);

            _logger.LogInformation("Cache SET: {Key} (TTL: {TTL}s)", key, ttl.TotalSeconds);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Cache REMOVE: {Key}", key);
        }
    }
}
