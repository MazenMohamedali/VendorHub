using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.Favorite;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;

public class FavoriteService : IFavoriteService
{
    private readonly IGeneralRepository<Favorite> _favoriteRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IGeneralRepository<Favorite> favoriteRepository,
        ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<GeneralResponse<string?>> AddToFavoritesAsync(int productId, int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var favorite = new Favorite
            {
                ProductId = productId,
                CustomerId = customerId,
                AddedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.SaveAsync(cancellationToken);

            _logger.LogInfoWithContext("Product successfully added to customer favorites.", new { CustomerId = customerId, ProductId = productId });

            return GeneralResponse<string?>.Created(null, "Added to favorites successfully.");
        } 
        catch (DbUpdateException ex)
        {
            _logger.LogWarningWithContext("Prevented duplicate favorite insertion due to database constraint violation.", new { ExceptionMessage = ex.Message, StackTrace = ex.StackTrace, CustomerId = customerId, ProductId = productId });
            return GeneralResponse<string?>.InvalidInput("Product is already in your favorites.");
        }
    }

    public async Task<GeneralResponse<string?>> RemoveFromFavoritesAsync(int productId, int customerId, CancellationToken cancellationToken)
    {
        var existing = await _favoriteRepository
                .GetBy(f => f.CustomerId == customerId && f.ProductId == productId)
                .FirstOrDefaultAsync(cancellationToken);

        if (existing == null)
        {
            var payload = new { CustomerId = customerId, ProductId = productId };
            _logger.LogWarningWithContext("Favorite removal rejected: Association does not exist.", payload);
            return GeneralResponse<string?>.NotFound("Product is not in your favorites.");
        }

        _favoriteRepository.Delete(existing);
        await _favoriteRepository.SaveAsync(cancellationToken);

        return GeneralResponse<string?>.Succeeded(null, "Removed successfully.");
    }

    public async Task<GeneralResponse<IEnumerable<FavoriteDto>>> GetCustomerFavoritesAsync(int customerId, CancellationToken cancellationToken)
    {
        var favorites = await _favoriteRepository
            .GetByAsNoTracking(f => f.CustomerId == customerId)
            .Select(FavoriteMapping())
            .ToListAsync(cancellationToken);

        string message = favorites.Count > 0 ? "Favorites retrieved successfully." : "No favorites found.";
        return GeneralResponse<IEnumerable<FavoriteDto>>.Succeeded(favorites, message);
    }

    private static Expression<Func<Favorite, FavoriteDto>> FavoriteMapping()
    {
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
}
