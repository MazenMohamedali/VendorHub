namespace VendorHub.Services.Caching
{
    public static class CacheKeys
    {
        public const string TOP_PRODUCTS = "top_products_trending";
        public static readonly TimeSpan TOP_PRODUCTS_TTL = TimeSpan.FromMinutes(5);
    }
}