using VendorHub.Services.Caching;

namespace VendorHub.Extensions
{
    public class Context
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static ICacheService CurrentService
        {
            get
            {
                var httpContext = _httpContextAccessor?.HttpContext
                    ?? throw new InvalidOperationException("HttpContext is not available. Ensure IHttpContextAccessor is registered.");

                return httpContext.RequestServices.GetRequiredService<ICacheService>();
            }
        }
    }
}
