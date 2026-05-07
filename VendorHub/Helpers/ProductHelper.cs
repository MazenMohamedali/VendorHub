using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using VendorHub.DTOs.ProductDto;
using VendorHub.Models;
using VendorHub.Settings;

namespace VendorHub.Helpers
{
    public class ProductHelper
    {
        public static string? BaseImageUrl { get; set; }
        public ProductHelper() { }

        public double CalculateAverageStars(int reviewCount, double overallStars)
        {
            return reviewCount > 0 ? (double)overallStars / reviewCount : 0;
        }

        public string GetImageUrl(string imgUrl)
        {
            return $"{BaseImageUrl}/{imgUrl}";
        }
    }
}
