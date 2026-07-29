using VendorHub.DTOs.ProductDto;

namespace VendorHub.DTOs.Favorite
{
    public class FavoriteDto : ProductBasicInfoDto
    {
        public DateTime AddedAt { get; set; }
        public double AverageStars { get; set; }
    }
}
