using System.Linq.Expressions;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface IProductService
    {
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetHotProductsAsync(
            int count = 6,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductCardDto>>> GetFilteredProductsAsync(
            Expression<Func<Product, bool>> filter,
            int page = 1,
            int pageSize = 10,
            string errorMessage = "No products found matching your criteria",
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductCardDto>>> SearchByNameAsync(
            string name,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductCardDto>>> SearchByCategoryAsync(
            string category,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductCardDto>>> SearchByPriceAsync(
            long min,
            long max,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductCardDto>>> GetProductsCardByCategoryIdAsync(
            int categoryId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductCardDto>>> GetProductsCardAsync(
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<ProductDetailsDto>> GetProductDetailsForCustomerAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<ProductDetailsWithStatusDto>> AddAsync(
            AddProductDto productFromRequest,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<ProductDetailsWithStatusDto>> UpdateAsync(
            EditProductDto productFromRequest,
            int productId,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> DeleteProductAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> ApproveProductAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> RejectProductAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<ProductDetailsWithStatusDto>> GetProductDetailsForVendorAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductDetailsWithStatusDto>>> GetVendorProductsListAsync(
            int vendorId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<ProductAdminDetailsDto>> GetProductDetailsForAdminAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<PagedResult<ProductAdminDetailsDto>>> GetAllProductsForAdminAsync(
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
