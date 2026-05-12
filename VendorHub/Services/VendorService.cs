using Microsoft.EntityFrameworkCore;
using VendorHub.DTOs.Vendors;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class VendorService
    {
        private readonly IGeneralRepository<Vendor> _vendorRepository;

        public VendorService(IGeneralRepository<Vendor> vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        public async Task<GeneralResponse<IEnumerable<VendorDetailsDto>>> GetAllVendorsAsync()
        {
            var vendorData = await _vendorRepository
                .GetAll()
                .Select(v => new VendorDetailsDto
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
                })
                .ToListAsync();

            return new GeneralResponse<IEnumerable<VendorDetailsDto>>()
                .Succeeded(vendorData, "Vendors retrieved successfully");
        }
    }
}