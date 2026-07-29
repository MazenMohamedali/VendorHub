using VendorHub.DTOs.Favorite;
using VendorHub.DTOs.sharedDto;

public interface IFavoriteService
{
    Task<GeneralResponse<string?>> AddToFavoritesAsync(int productId, int customerId, CancellationToken cancellationToken);
    Task<GeneralResponse<IEnumerable<FavoriteDto>>> GetCustomerFavoritesAsync(int customerId, CancellationToken cancellationToken);
    Task<GeneralResponse<string?>> RemoveFromFavoritesAsync(int productId, int customerId, CancellationToken cancellationToken);
}
