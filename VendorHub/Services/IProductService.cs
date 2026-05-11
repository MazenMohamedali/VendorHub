using System.Linq.Expressions;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface IProductService
    {
        public Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetHotProductsAsync(int count);
        Task<GeneralResponse<ProductDetailsWithStatusDto>> AddAsync(AddProductDto productFromRequest);
        Task<GeneralResponse> ApproveProductAsync(int id);
        Task<GeneralResponse> DeleteProductAsync(int id);
        Task<GeneralResponse<IEnumerable<ProductAdminDetailsDto>>> GetAllProductsForAdminAsync();
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetFilteredProductsAsync(Expression<Func<Product, bool>> filter, string errorMessage = "No products found matching your criteria");
        Task<GeneralResponse<ProductAdminDetailsDto>> GetProductDetailsForAdminAsync(int id);
        Task<GeneralResponse<ProductDetailsDto>> GetProductDetailsForCustomerAsync(int id);
        Task<GeneralResponse<ProductDetailsWithStatusDto>> GetProductDetailsForVendorAsync(int id);
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetProductsCardAsync();
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetProductsCardByCategoryId(int categoryId);
        Task<GeneralResponse<IEnumerable<ProductDetailsWithStatusDto>>> GetVendorProductsListAsync(int vendorId);
        Task<GeneralResponse> RejectProductAsync(int id);
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> SearchByCategoryAsync(string category);
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> SearchByNameAsync(string name);
        Task<GeneralResponse<IEnumerable<ProductCardDto>>> SearchByPriceAsync(long min, long max);
        Task<GeneralResponse<ProductDetailsWithStatusDto>> UpdateAsync(EditProductDto productFromRequest, int productId);
    }
}