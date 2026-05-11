using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Helpers;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;
using VendorHub.Settings;

namespace VendorHub.Services
{
    public class ProductService : IProductService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ProductHelper _productHelper;
        private readonly ICacheService _cacheService;

        public ProductService(IWebHostEnvironment webHostEnvironment, IOptions<JwtOptions> options, IGeneralRepository<Product> productRepository, ICacheService cacheService)
        {
            _webHostEnvironment = webHostEnvironment;
            _productRepository = productRepository;
            _productHelper = new ProductHelper();
            _cacheService = cacheService;
        }

        #region Hot Product With Caching
        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetHotProductsAsync(int count = 6)
        {
            var products = await _cacheService.GetOrSetAsync(
                key: CacheKeys.TOP_PRODUCTS,
                factory: async () =>
                {
                    return await _productRepository.GetAll()
                        .Where(p => p.Status == ProductStatus.REVIEWED)
                        .OrderByDescending(p => p.ViewersNo)
                        .ThenByDescending(p => p.OverallStars)
                        .Take(count)
                        .Select(ProductToCardDto())
                        .ToListAsync();
                },
                expiration: CacheKeys.TOP_PRODUCTS_TTL
            );

            return new GeneralResponse<IEnumerable<ProductCardDto>>().Succeeded(products);
        }
        #endregion

        #region Filter Products 
        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetFilteredProductsAsync(Expression<Func<Product, bool>> filter, string errorMessage = "No products found matching your criteria")
        {
            var products = await _productRepository
                .GetAll()
                .Where(p => p.Status == ProductStatus.REVIEWED)
                .Where(filter)
                .Select(ProductToCardDto())
                .ToListAsync();

            if (products == null || !products.Any())
            {
                return new GeneralResponse<IEnumerable<ProductCardDto>>().Failed(errorMessage);
            }

            return new GeneralResponse<IEnumerable<ProductCardDto>>()
                .Succeeded(products, "Products retrieved successfully");
        }

        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> SearchByNameAsync(string name)
        {
            return await GetFilteredProductsAsync(p => p.Name.Contains(name), $"No products found with name: {name}");
        }

        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> SearchByCategoryAsync(string category)
        {
            return await GetFilteredProductsAsync(p => p.Category.Name.Contains(category), $"No products found in category: {category}");
        }

        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> SearchByPriceAsync(long min, long max)
        {
            return await GetFilteredProductsAsync(p => p.Price >= min && p.Price <= max, $"No products found between {min} and {max}");
        }

        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetProductsCardByCategoryId(int categoryId)
        {
            return await GetFilteredProductsAsync(p => p.CategoryId == categoryId, "No products found for this category");
        }
        #endregion

        #region Command Methods (Add, Update, Delete, Approve)
        public async Task<GeneralResponse<ProductDetailsWithStatusDto>> AddAsync(AddProductDto productFromRequest)
        {
            var product = productFromRequest.GetProduct();

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            if (productFromRequest.ImageFile != null)
                await AddImageToProduct(productFromRequest.ImageFile, product);

            var result = ProductToVendorDetailsDto().Compile()(product);
            return new GeneralResponse<ProductDetailsWithStatusDto>().Succeeded(result, "Product created and pending admin approval");
        }

        public async Task<GeneralResponse<ProductDetailsWithStatusDto>> UpdateAsync(EditProductDto productFromRequest, int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is null)
                return new GeneralResponse<ProductDetailsWithStatusDto>().Failed("Product not found");

            await productFromRequest.ApplyTo(product);

            if (productFromRequest.ImageFile != null)
            {
                string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Products", product.ImgUrl ?? "");
                if (File.Exists(oldPath)) File.Delete(oldPath);

                await AddImageToProduct(productFromRequest.ImageFile, product);
            }

            await _productRepository.SaveAsync();
            var result = ProductToVendorDetailsDto().Compile()(product);
            return new GeneralResponse<ProductDetailsWithStatusDto>().Succeeded(result, "Product updated successfully");
        }

        public async Task<GeneralResponse> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return new GeneralResponse().Failed("Product not found");

            product.Status = ProductStatus.Archived;
            await _productRepository.SaveAsync();
            return new GeneralResponse().Succeeded("Product archived successfully");
        }

        public async Task<GeneralResponse> ApproveProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return new GeneralResponse().Failed("Product not found");

            product.Status = ProductStatus.REVIEWED;
            await _productRepository.SaveAsync();
            return new GeneralResponse().Succeeded("Product approved and is now visible to customers");
        }

        public async Task<GeneralResponse> RejectProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return new GeneralResponse().Failed("Product not found");

            product.Status = ProductStatus.REJECTED;
            await _productRepository.SaveAsync();
            return new GeneralResponse().Succeeded("Product rejected");
        }
        #endregion

        #region Get Methods
        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetProductsCardAsync()
        {
            var products = await _productRepository.GetAll()
                .Where(p => p.Status == ProductStatus.REVIEWED)
                .Select(ProductToCardDto())
                .ToListAsync();
            return new GeneralResponse<IEnumerable<ProductCardDto>>().Succeeded(products);
        }

        public async Task<GeneralResponse<ProductDetailsDto>> GetProductDetailsForCustomerAsync(int id)
        {
            var product = await _productRepository.GetAll()
                .Where(p => p.Id == id && p.Status == ProductStatus.REVIEWED)
                .Select(ProductToCustomerDetailsDto())
                .FirstOrDefaultAsync();

            return product == null
                ? new GeneralResponse<ProductDetailsDto>().Failed("Product not available")
                : new GeneralResponse<ProductDetailsDto>().Succeeded(product);
        }

        public async Task<GeneralResponse<ProductDetailsWithStatusDto>> GetProductDetailsForVendorAsync(int id)
        {
            var product = await _productRepository.GetAll()
                .Where(p => p.Id == id)
                .Select(ProductToVendorDetailsDto())
                .FirstOrDefaultAsync();

            return product == null
                ? new GeneralResponse<ProductDetailsWithStatusDto>().Failed("Product not found")
                : new GeneralResponse<ProductDetailsWithStatusDto>().Succeeded(product);
        }

        public async Task<GeneralResponse<IEnumerable<ProductDetailsWithStatusDto>>> GetVendorProductsListAsync(int vendorId)
        {
            var products = await _productRepository.GetAll()
                .Where(p => p.VendorId == vendorId)
                .Select(ProductToVendorDetailsDto())
                .ToListAsync();

            return new GeneralResponse<IEnumerable<ProductDetailsWithStatusDto>>().Succeeded(products);
        }

        public async Task<GeneralResponse<ProductAdminDetailsDto>> GetProductDetailsForAdminAsync(int id)
        {
            var product = await _productRepository.GetAll()
                .Where(p => p.Id == id)
                .Select(ProductToAdminDetailsDto())
                .FirstOrDefaultAsync();

            return product == null
                ? new GeneralResponse<ProductAdminDetailsDto>().Failed("Product not found")
                : new GeneralResponse<ProductAdminDetailsDto>().Succeeded(product);
        }

        public async Task<GeneralResponse<IEnumerable<ProductAdminDetailsDto>>> GetAllProductsForAdminAsync()
        {
            var products = await _productRepository.GetAll()
                .Select(ProductToAdminDetailsDto())
                .ToListAsync();
            return new GeneralResponse<IEnumerable<ProductAdminDetailsDto>>().Succeeded(products);
        }
        #endregion

        #region Private Helpers
        private Expression<Func<Product, ProductCardDto>> ProductToCardDto()
        {
            return p => new ProductCardDto
            {
                Id = p.Id,
                ImgUrl = _productHelper.GetImageUrl(p.ImgUrl),
                Price = p.Price,
                Name = p.Name,
                ViewersNo = p.ViewersNo,
                AverageStars = _productHelper.CalculateAverageStars(p.ReviewCount, p.OverallStars)
            };
        }

        private Expression<Func<Product, ProductDetailsDto>> ProductToCustomerDetailsDto()
        {
            return p => new ProductDetailsDto
            {
                Id = p.Id,
                ImgUrl = _productHelper.GetImageUrl(p.ImgUrl),
                Price = p.Price,
                Name = p.Name,
                ViewersNo = p.ViewersNo,
                AverageStars = _productHelper.CalculateAverageStars(p.ReviewCount, p.OverallStars),
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                storeName = p.Vendor.StoreName,
                CategoryName = p.Category.Name,
            };
        }

        private Expression<Func<Product, ProductDetailsWithStatusDto>> ProductToVendorDetailsDto()
        {
            return p => new ProductDetailsWithStatusDto
            {
                Id = p.Id,
                ImgUrl = _productHelper.GetImageUrl(p.ImgUrl),
                Price = p.Price,
                Name = p.Name,
                ViewersNo = p.ViewersNo,
                AverageStars = _productHelper.CalculateAverageStars(p.ReviewCount, p.OverallStars),
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                storeName = p.Vendor.StoreName,
                CategoryName = p.Category.Name,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ReviewCount = p.ReviewCount,
            };
        }

        private Expression<Func<Product, ProductAdminDetailsDto>> ProductToAdminDetailsDto()
        {
            return p => new ProductAdminDetailsDto
            {
                Id = p.Id,
                ImgUrl = _productHelper.GetImageUrl(p.ImgUrl),
                Price = p.Price,
                Name = p.Name,
                ViewersNo = p.ViewersNo,
                AverageStars = _productHelper.CalculateAverageStars(p.ReviewCount, p.OverallStars),
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                storeName = p.Vendor.StoreName,
                CategoryName = p.Category.Name,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ReviewCount = p.ReviewCount,
                VendorId = p.VendorId,
                CategoryId = p.CategoryId,
            };
        }

        private async Task AddImageToProduct(IFormFile ImageFile, Product productFromDB)
        {
            string extension = Path.GetExtension(ImageFile.FileName);
            string fileName = $"{productFromDB.Id}{extension}";
            string imagesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Products");

            if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

            string filePath = Path.Combine(imagesFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            productFromDB.ImgUrl = fileName;
            await _productRepository.UpdateAsync(productFromDB);
            await _productRepository.SaveAsync();
        }
        #endregion
    }
}
