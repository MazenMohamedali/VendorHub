using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.Favorite;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<ActionResult<GeneralResponse<IEnumerable<FavoriteDto>>>> GetMyFavorites(CancellationToken cancellationToken = default)
        {
            var customerId = this.GetUserId();
            var result = await _favoriteService.GetCustomerFavoritesAsync(customerId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("product/{productId:int}")]
        public async Task<ActionResult<GeneralResponse<string?>>> AddToFavorites(int productId, CancellationToken cancellationToken = default)
        {
            var customerId = this.GetUserId();
            var result = await _favoriteService.AddToFavoritesAsync(productId, customerId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpDelete("product/{productId:int}")]
        public async Task<ActionResult<GeneralResponse<string?>>> RemoveFromFavorites(int productId, CancellationToken cancellationToken = default)
        {
            var customerId = this.GetUserId();
            var result = await _favoriteService.RemoveFromFavoritesAsync(productId, customerId, cancellationToken);
            return this.HandleResult(result);
        }
    }
}
