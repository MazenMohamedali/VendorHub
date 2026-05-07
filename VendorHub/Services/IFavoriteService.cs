using VendorHub.DTOs.Favorite;
using VendorHub.DTOs.sharedDto;

public interface IFavoriteService
{
    Task<GeneralResponse<string?>> AddToFavoritesAsync(int productId, int customerId);
    Task<GeneralResponse<IEnumerable<FavoriteDto>>> GetCustomerFavoritesAsync(int customerId);
    Task<GeneralResponse<string?>> RemoveFromFavoritesAsync(int productId, int customerId);
}