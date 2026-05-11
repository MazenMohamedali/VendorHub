using VendorHub.DTOs.ProductDto;
using VendorHub.Services;

namespace VendorHub.GraphQL
{
    public class Query
    {
        public async Task<IEnumerable<ProductCardDto>> GetHotProducts(
        [Service] IProductService productService,
        int count = 6)
        {
            var response = await productService.GetHotProductsAsync(count);

            if (response.Success)
            {
                return response.Data;
            }

            return Enumerable.Empty<ProductCardDto>();
        }
    }
}
