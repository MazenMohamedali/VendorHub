using Microsoft.EntityFrameworkCore;
using VendorHub.Services.Caching;

namespace VendorHub.Extensions
{
    public static class CacheExtensions
    {
        public static async Task<List<T>> ToCachedListAsync<T>(
            this IQueryable<T> query,
            ICacheService cacheService,
            string cacheKey,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            var (found, cached) = await cacheService.TryGetAsync<List<T>>(cacheKey, expiration, cancellationToken);

            if (found && cached != null)
                return cached;

            var result = await query.ToListAsync(cancellationToken);
            if (result != null)
                await cacheService.SetAsync(cacheKey, result, expiration, cancellationToken);

            return result ?? new List<T>();
        }

        public static async Task<T?> ToCachedFirstOrDefaultAsync<T>(
            this IQueryable<T> query,
            ICacheService cacheService,
            string cacheKey,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            var (found, cached) = await cacheService.TryGetAsync<T>(cacheKey, expiration, cancellationToken);
            if (found && cached != null)
                return cached;

            var result = await query.FirstOrDefaultAsync(cancellationToken);
            if (result != null)
            {
                await cacheService.SetAsync(cacheKey, result, expiration, cancellationToken);
            }

            return result;
        }
    }
}
