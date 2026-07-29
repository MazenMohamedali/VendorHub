using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.Constants;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;
using VendorHub.Services.Storage;

namespace VendorHub.Services
{
    public class ProductService : IProductService
    {
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ICacheService _cacheService;
        private readonly IFileService _fileService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IGeneralRepository<Product> productRepository,
            ICacheService cacheService,
            IFileService fileService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _fileService = fileService;
            _logger = logger;
        }

        #region Hot Products With Caching
        public async Task<GeneralResponse<IEnumerable<ProductCardDto>>> GetHotProductsAsync(int count = 6, CancellationToken cancellationToken = default)
        {
            var products = await _cacheService.GetOrSetAsync(
                key: CacheKeys.TOP_PRODUCTS,
                factory: () => _productRepository.GetAll()
                    .AsNoTracking()
                    .Where(p => p.Status == ProductStatus.REVIEWED)
                    .OrderByDescending(p => p.ViewersNo)
                    .ThenByDescending(p => p.OverallStars)
                    .Take(count)
                    .Select(ProductToCardDto())
                    .ToListAsync(cancellationToken),
                expiration: CacheKeys.TOP_PRODUCTS_TTL
            );

            return GeneralResponse<IEnumerable<ProductCardDto>>.Succeeded(products ?? []);
        }
        #endregion

        #region Filter Products 
        public async Task<GeneralResponse<PagedResult<ProductCardDto>>> GetFilteredProductsAsync(Expression<Func<Product, bool>> filter, int page = 1, int pageSize = 10, string errorMessage = "No products found matching your criteria", CancellationToken cancellationToken = default)
        {
            var pagedResult = await _productRepository.GetBy(p => p.Status == ProductStatus.REVIEWED)
                    .AsNoTracking()
                    .Where(filter)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToPagedResultAsync(ProductToCardDto(), page, pageSize, cancellationToken);

            return GeneralResponse<PagedResult<ProductCardDto>>.Succeeded(pagedResult, "Products retrieved successfully");
        }

        public Task<GeneralResponse<PagedResult<ProductCardDto>>> SearchByNameAsync(string name, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return GetFilteredProductsAsync(p => p.Name.Contains(name), page, pageSize, $"No products found matching: {name}", cancellationToken);
        }

        public Task<GeneralResponse<PagedResult<ProductCardDto>>> SearchByCategoryAsync(string category, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return GetFilteredProductsAsync(p => p.Category.Name.Contains(category), page, pageSize, $"No products found in category: {category}", cancellationToken);
        }

        public Task<GeneralResponse<PagedResult<ProductCardDto>>> SearchByPriceAsync(long min, long max, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return GetFilteredProductsAsync(p => p.Price >= min && p.Price <= max, page, pageSize, $"No products found between {min} and {max}", cancellationToken);
        }

        public Task<GeneralResponse<PagedResult<ProductCardDto>>> GetProductsCardByCategoryIdAsync(int categoryId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return GetFilteredProductsAsync(p => p.CategoryId == categoryId, page, pageSize, "No products found for this category", cancellationToken);
        }
        #endregion

        #region Command Methods (Add, Update, Delete, Approve)
        public async Task<GeneralResponse<ProductDetailsWithStatusDto>> AddAsync(AddProductDto productFromRequest, CancellationToken cancellationToken = default)
        {
            var product = productFromRequest.GetProduct();

            if (productFromRequest.ImageFile != null)
            {
                product.ImgUrl = await _fileService.SaveImageAsync(productFromRequest.ImageFile, ImageFolders.Products);
            }

            await _productRepository.AddAsync(product, cancellationToken);
            await _productRepository.SaveAsync(cancellationToken);

            _logger.LogInformation("Product {ProductId} created by Vendor {VendorId}", product.Id, product.VendorId);

            var result = MapToVendorDetailsDto(product);
            return GeneralResponse<ProductDetailsWithStatusDto>.Created(result, "Product created and pending admin approval");
        }

        public async Task<GeneralResponse<ProductDetailsWithStatusDto>> UpdateAsync(EditProductDto productFromRequest, int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product is null)
                    return GeneralResponse<ProductDetailsWithStatusDto>.NotFound("Product not found");

                await productFromRequest.ApplyTo(product);

                if (productFromRequest.ImageFile != null)
                    product.ImgUrl = await _fileService.ReplaceImageAsync(product.ImgUrl, productFromRequest.ImageFile, ImageFolders.Products);

                await _productRepository.SaveAsync(cancellationToken);

                await _cacheService.RemoveAsync(CacheKeys.TOP_PRODUCTS, cancellationToken);
                await _cacheService.RemoveAsync(CacheKeys.ProductDetails(productId), cancellationToken);
                _logger.LogInformation("Product {ProductId} updated", productId);

                var result = MapToVendorDetailsDto(product);
                return GeneralResponse<ProductDetailsWithStatusDto>.Succeeded(result, "Product updated successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict detected while updating Product {ProductId}", productId);
                return GeneralResponse<ProductDetailsWithStatusDto>.Error("The product was modified by another user. Please refresh and try again.");
            }
        }

        public async Task<GeneralResponse> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return GeneralResponse.NotFound("Product not found");

            product.Status = ProductStatus.Archived;
            await _productRepository.SaveAsync(cancellationToken);

            await _cacheService.RemoveAsync(CacheKeys.TOP_PRODUCTS, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.ProductDetails(id), cancellationToken);
            _logger.LogInformation("Product {ProductId} archived", id);

            return GeneralResponse.Succeeded("Product archived successfully");
        }

        public async Task<GeneralResponse> ApproveProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Admin attempt to approve non-existent Product {ProductId}", id);
                return GeneralResponse.NotFound("Product not found");
            }

            product.Status = ProductStatus.REVIEWED;
            await _productRepository.SaveAsync(cancellationToken);

            await _cacheService.RemoveAsync(CacheKeys.TOP_PRODUCTS, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.ProductDetails(id), cancellationToken);
            _logger.LogInformation("Product {ProductId} approved by Admin", id);

            return GeneralResponse.Succeeded("Product approved and is now visible to customers");
        }

        public async Task<GeneralResponse> RejectProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return GeneralResponse.NotFound("Product not found");

            product.Status = ProductStatus.REJECTED;
            await _productRepository.SaveAsync(cancellationToken);

            await _cacheService.RemoveAsync(CacheKeys.TOP_PRODUCTS, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.ProductDetails(id), cancellationToken);
            _logger.LogInformation("Product {ProductId} rejected by Admin", id);

            return GeneralResponse.Succeeded("Product rejected");
        }
        #endregion

        #region Get Methods
        public async Task<GeneralResponse<PagedResult<ProductCardDto>>> GetProductsCardAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return await GetFilteredProductsAsync(p => true, page, pageSize, "No active products available", cancellationToken);
        }

        public async Task<GeneralResponse<ProductDetailsDto>> GetProductDetailsForCustomerAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository
                .GetBy(p => p.Id == id && p.Status == ProductStatus.REVIEWED)
                .AsNoTracking()
                .Select(ProductToCustomerDetailsDto())
                .ToCachedFirstOrDefaultAsync(_cacheService, CacheKeys.ProductDetails(id), CacheKeys.ProductDetails_TTL, cancellationToken);

            return product == null
                ? GeneralResponse<ProductDetailsDto>.NotFound("Product not available")
                : GeneralResponse<ProductDetailsDto>.Succeeded(product);
        }

        public async Task<GeneralResponse<ProductDetailsWithStatusDto>> GetProductDetailsForVendorAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository
                .GetBy(p => p.Id == id)
                .AsNoTracking()
                .Select(ProductToVendorDetailsDto())
                .FirstOrDefaultAsync(cancellationToken);

            return product == null
                ? GeneralResponse<ProductDetailsWithStatusDto>.NotFound("Product not found")
                : GeneralResponse<ProductDetailsWithStatusDto>.Succeeded(product);
        }

        public async Task<GeneralResponse<PagedResult<ProductDetailsWithStatusDto>>> GetVendorProductsListAsync(int vendorId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _productRepository.GetAll()
                .AsNoTracking()
                .Where(p => p.VendorId == vendorId)
                .ToPagedResultAsync(ProductToVendorDetailsDto(), page, pageSize, cancellationToken);

            return GeneralResponse<PagedResult<ProductDetailsWithStatusDto>>.Succeeded(pagedResult);
        }

        public async Task<GeneralResponse<ProductAdminDetailsDto>> GetProductDetailsForAdminAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository
                .GetBy(p => p.Id == id)
                .Select(ProductToAdminDetailsDto())
                .FirstOrDefaultAsync(cancellationToken);

            return product == null
                ? GeneralResponse<ProductAdminDetailsDto>.NotFound("Product not found")
                : GeneralResponse<ProductAdminDetailsDto>.Succeeded(product);
        }

        public async Task<GeneralResponse<PagedResult<ProductAdminDetailsDto>>> GetAllProductsForAdminAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _productRepository.GetAll()
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToPagedResultAsync(ProductToAdminDetailsDto(), page, pageSize, cancellationToken);

            return GeneralResponse<PagedResult<ProductAdminDetailsDto>>.Succeeded(pagedResult);
        }
        #endregion

        #region Private Helpers
        private Expression<Func<Product, ProductCardDto>> ProductToCardDto()
        {
            return p => new ProductCardDto
            {
                Id = p.Id,
                ImgUrl = _fileService.BuildImageUrl(ImageFolders.Products, p.ImgUrl),
                Price = p.Price,
                Name = p.Name,
                AverageStars = p.ReviewCount > 0
                    ? (double)p.OverallStars / p.ReviewCount
                    : 0,
                ViewersNo = p.ViewersNo
            };
        }

        private Expression<Func<Product, ProductDetailsDto>> ProductToCustomerDetailsDto()
        {
            return p => new ProductDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImgUrl = _fileService.BuildImageUrl(ImageFolders.Products, p.ImgUrl),
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                storeName = p.Vendor != null ? p.Vendor.StoreName : string.Empty,
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                AverageStars = p.ReviewCount > 0
                    ? (double)p.OverallStars / p.ReviewCount
                    : 0,
                ViewersNo = p.ViewersNo
            };
        }

        private Expression<Func<Product, ProductDetailsWithStatusDto>> ProductToVendorDetailsDto()
        {
            return p => new ProductDetailsWithStatusDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImgUrl = _fileService.BuildImageUrl(ImageFolders.Products, p.ImgUrl),
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                storeName = p.Vendor != null ? p.Vendor.StoreName : string.Empty,
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ReviewCount = p.ReviewCount,
                AverageStars = p.ReviewCount > 0
                    ? (double)p.OverallStars / p.ReviewCount
                    : 0,
                ViewersNo = p.ViewersNo
            };
        }

        private Expression<Func<Product, ProductAdminDetailsDto>> ProductToAdminDetailsDto()
        {
            return p => new ProductAdminDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImgUrl = _fileService.BuildImageUrl(ImageFolders.Products, p.ImgUrl),
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                storeName = p.Vendor != null ? p.Vendor.StoreName : string.Empty,
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ReviewCount = p.ReviewCount,
                VendorId = p.VendorId,
                CategoryId = p.CategoryId,
                AverageStars = p.ReviewCount > 0
                    ? (double)p.OverallStars / p.ReviewCount
                    : 0,
                ViewersNo = p.ViewersNo
            };
        }

        private ProductDetailsWithStatusDto MapToVendorDetailsDto(Product p)
        {
            return new ProductDetailsWithStatusDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImgUrl = _fileService.BuildImageUrl(ImageFolders.Products, p.ImgUrl),
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                storeName = p.Vendor != null ? p.Vendor.StoreName : string.Empty,
                UnitsInStock = p.Quantity,
                ProductionDate = p.ProductionDate,
                ExpireDate = p.ExpireDate,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ReviewCount = p.ReviewCount,
                AverageStars = p.ReviewCount > 0
                    ? (double)p.OverallStars / p.ReviewCount
                    : 0,
                ViewersNo = p.ViewersNo
            };
        }
        #endregion
    }
}
