namespace VendorHub.Services.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task<(bool Found, T? Value)> TryGetAsync<T>(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
