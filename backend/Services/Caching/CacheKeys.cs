namespace VendorHub.Services.Caching
{
    public static class CacheKeys
    {
        // -------------------------------------------------------------
        // Categories Keys & TTLs
        // -------------------------------------------------------------
        public const string ALL_CATEGORIES = "categories:all";
        public static string CategoryDetails(int id) => $"category:{id}";
        
        public static readonly TimeSpan CategoriesL2_TTL = TimeSpan.FromDays(30); // Perpetual L2 Redis Cache
        public static readonly TimeSpan CategoriesL1_TTL = TimeSpan.FromMinutes(15); // L1 Safety TTL for multi-instance sync

        // -------------------------------------------------------------
        // Products Keys & TTLs
        // -------------------------------------------------------------
        public const string TOP_PRODUCTS = "products:hot_trending";
        public static string ProductDetails(int id) => $"product:{id}";

        public static readonly TimeSpan TOP_PRODUCTS_TTL = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan ProductDetails_TTL = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan ProductDetailsL1_TTL = TimeSpan.FromMinutes(5);

        // -------------------------------------------------------------
        // Vendor Permissions Keys & TTLs
        // -------------------------------------------------------------
        public static string VendorPermissions(int vendorId) => $"vendor:permissions:{vendorId}";
        public static readonly TimeSpan VendorPermissions_TTL = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan VendorPermissionsL1_TTL = TimeSpan.FromMinutes(10);

        // -------------------------------------------------------------
        // Statistics Keys & TTLs
        // -------------------------------------------------------------
        public static string VendorStats(int vendorId) => $"stats:vendor:{vendorId}";
        public static readonly TimeSpan VendorStats_TTL = TimeSpan.FromMinutes(5);
    }
}
