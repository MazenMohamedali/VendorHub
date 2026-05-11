using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
 
namespace VendorHub.Services.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T cachedValue))
                {
                    _logger.LogInformation($"Cache HIT (L1): {key}");
                    return cachedValue;
                }

                var redisData = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(redisData))
                {
                    var value = JsonSerializer.Deserialize<T>(redisData);
                    _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                    _logger.LogInformation($"Cache HIT (L2): {key}");
                    return value;
                }

                _logger.LogInformation($"Cache MISS: {key}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cache error reading {key}: {ex.Message}");
                return default;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            TimeSpan ttl = expiration ?? TimeSpan.FromMinutes(10);

            var cached = await GetAsync<T>(key);
            if (cached != null)
                return cached;

            _logger.LogInformation($"Executing factory for: {key}");
            var value = await factory();

            if (value != null)
                await SetAsync(key, value, ttl);

            return value;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var ttl = expiration ?? TimeSpan.FromMinutes(10);

                _memoryCache.Set(key, value, ttl);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };

                string json = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, json, cacheOptions);

                _logger.LogInformation($"Cache SET: {key} (TTL: {ttl.TotalSeconds}s)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cache error writing {key}: {ex.Message}");
            }
        }
    }
}