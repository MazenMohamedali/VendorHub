using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.sharedDto;
namespace VendorHub.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<TDestination>> ToPagedResultAsync<TSource,  TDestination>(this IQueryable<TSource> query, Expression<Func<TSource, TDestination>> projection, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = new PagedResult<TDestination>()
            {
                Page = page, 
                PageSize = pageSize, 
            };

            result.TotalCount = await query.CountAsync(cancellationToken);
            if (result.TotalCount == 0)
                return result;

            result.Items = await query
                .Skip(result.SkipCount)
                .Take(result.PageSize)
                .Select(projection)
                .ToListAsync(cancellationToken);

            return result;
        }

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = new PagedResult<T>()
            {
                Page = page,
                PageSize = pageSize,
            };

            result.TotalCount = await query.CountAsync(cancellationToken);
            if (result.TotalCount == 0)
                return result;

            result.Items = await query
                .Skip(result.SkipCount)
                .Take(result.PageSize)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
