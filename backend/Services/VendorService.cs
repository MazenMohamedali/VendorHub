using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.VendorDto;
using VendorHub.DTOs.Vendors;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class VendorService : BaseUserService<Vendor>, IVendorService
    {
        private readonly ILogger<VendorService> _logger;
        public VendorService(IGeneralRepository<Vendor> vendorRepository, ILogger<VendorService> logger) : base(vendorRepository)
        {
            _logger = logger;
        }

        public async Task<GeneralResponse<VendorProfileDto>> GetVendorProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching profile context for vendor {VendorId}", userId);

            var vendor = await _repository.GetByIdAsync(userId, cancellationToken);
            if (vendor == null)
            {
                _logger.LogWarningWithContext("Vendor profile lookup failed: Account not found for ID {VendorId}", new { VendorId = userId });
                return GeneralResponse<VendorProfileDto>.NotFound("Vendor account context not found.");
            }

            return GeneralResponse<VendorProfileDto>.Succeeded(MapToProfileDto(vendor), "Vendor profile retrieved successfully.");
        }

        public async Task<GeneralResponse<VendorProfileDto>> UpdateVendorProfileAsync(int userId, UpdateVendorProfileDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInfoWithContext("Attempting profile update for vendor {VendorId}", new { VendorId = userId });

            var vendor = await _repository.GetByIdAsync(userId, cancellationToken);
            if (vendor == null)
            {
                return GeneralResponse<VendorProfileDto>.NotFound("Vendor profile not found.");
            }

            try
            {
                MapCommonFields(vendor, dto.FirstName, dto.SecondName, dto.PhoneNumber);
                vendor.StoreName = dto.StoreName ?? vendor.StoreName;

                _repository.Update(vendor);
                await _repository.SaveAsync(cancellationToken);

                _logger.LogInfoWithContext("Successfully updated profile for vendor {VendorId}", new { VendorId = userId });
                return GeneralResponse<VendorProfileDto>.Succeeded(MapToProfileDto(vendor), "Profile updated successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogErrorWithContext("Concurrency conflict while updating vendor {VendorId}", ex, new { VendorId = userId });
                return GeneralResponse<VendorProfileDto>.Error("The profile was modified by another request. Please reload and try again.");
            }
        }

        public async Task<GeneralResponse<PagedResult<VendorDetailsDto>>> GetAllVendorsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _repository
                .GetAllAsNoTracking()
                .OrderByDescending(v => v.CreatedAt)
                .ToPagedResultAsync(v => new VendorDetailsDto
                {
                    Id = v.Id,
                    Email = v.Email ?? string.Empty,
                    FirstName = v.FirstName,
                    SecondName = v.SecondName,
                    StoreName = v.StoreName,
                    Balance = v.Balance,
                    AccountStatus = v.AccountStatus.ToString(),
                    CreatedAt = v.CreatedAt,
                    ProductCount = v.Products.Count
                }, page, pageSize, cancellationToken);

            return GeneralResponse<PagedResult<VendorDetailsDto>>.Succeeded(
                pagedResult,
                "Vendors loaded successfully.");
        }

        private VendorProfileDto MapToProfileDto(Vendor vendor)
        {
            return new VendorProfileDto
            {
                Id = vendor.Id,
                Email = vendor.Email ?? string.Empty,
                FirstName = vendor.FirstName,
                SecondName = vendor.SecondName,
                PhoneNumber = vendor.PhoneNumber ?? string.Empty,
                Role = "Vendor",
                AccountStatus = vendor.AccountStatus.ToString(),
                StoreName = vendor.StoreName,
                Balance = vendor.Balance
            };
        }
    }
}
