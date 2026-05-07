using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.Favorite;
using VendorHub.DTOs.sharedDto;
using VendorHub.Helpers;
using VendorHub.Models;
using VendorHub.Repository;

public class FavoriteService : IFavoriteService
{
    private readonly IGeneralRepository<Favorite> _favoriteRepository;
    private readonly ProductHelper _productHelper;

    public FavoriteService(IGeneralRepository<Favorite> favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
        _productHelper = new ProductHelper();
    }

    public async Task<GeneralResponse<string?>> AddToFavoritesAsync(int productId, int customerId)
    {
        var existing = await _favoriteRepository
            .GetByAsync(f => f.CustomerId == customerId && f.ProductId == productId);

        if (existing != null)
            return new GeneralResponse<string?>().Failed("Product is already in your favorites.");

        var favorite = new Favorite
        {
            ProductId = productId,
            CustomerId = customerId,
            AddedAt = DateTime.UtcNow
        };

        await _favoriteRepository.AddAsync(favorite);
        await _favoriteRepository.SaveAsync();

        return new GeneralResponse<string?>().Succeeded(null, "Added to favorites successfully.");
    }

    public async Task<GeneralResponse<string?>> RemoveFromFavoritesAsync(int productId, int customerId)
    {
        var existing = await _favoriteRepository
            .GetByAsync(f => f.CustomerId == customerId && f.ProductId == productId);

        if (existing == null)
            return new GeneralResponse<string?>().Failed("Product is not in your favorites.");

        await _favoriteRepository.DeleteAsync(existing);
        await _favoriteRepository.SaveAsync();

        return new GeneralResponse<string?>().Succeeded(null, "Removed from favorites successfully.");
    }

    public async Task<GeneralResponse<IEnumerable<FavoriteDto>>> GetCustomerFavoritesAsync(int customerId)
    {
        var favorites = await _favoriteRepository
            .GetBy(f => f.CustomerId == customerId)
            .Select(FavoriteMapping())
            .ToListAsync();

        if (favorites == null || !favorites.Any())
            return new GeneralResponse<IEnumerable<FavoriteDto>>().Succeeded(new List<FavoriteDto>(), "No favorites found.");

        return new GeneralResponse<IEnumerable<FavoriteDto>>().Succeeded(favorites, "Favorites retrieved successfully.");
    }

    #region Private Helpers
    private Expression<Func<Favorite, FavoriteDto>> FavoriteMapping()
    {
        string baseUrl = $"{ProductHelper.BaseImageUrl}/";

        return f => new FavoriteDto
        {
            Id = f.Product.Id,
            Name = f.Product.Name,
            Price = f.Product.Price,
            AddedAt = f.AddedAt,
            ImgUrl = f.Product.ImgUrl,
            AverageStars = f.Product.ReviewCount > 0
                ? (double)f.Product.OverallStars / f.Product.ReviewCount
                : 0
        };
    }
    #endregion
}