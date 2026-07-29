using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VendorHub.DTOs.CustomerDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class CustomerService : BaseUserService<Customer>, ICustomerService
    {
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(
            IGeneralRepository<Customer> customerRepository,
            ILogger<CustomerService> logger) : base(customerRepository)
        {
            _logger = logger;
        }

        public async Task<GeneralResponse<CustomerProfileDto>> GetCustomerProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            var customer = await _repository
                .GetByAsNoTracking(u => u.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                _logger.LogWarningWithContext("Profile lookup failed: Account does not exist.", new { UserId = userId });
                return GeneralResponse<CustomerProfileDto>.NotFound("Customer account not found.");
            }

            return GeneralResponse<CustomerProfileDto>.Succeeded(MapToProfileDto(customer), "Customer profile retrieved successfully.");
        }

        public async Task<GeneralResponse<CustomerProfileDto>> UpdateCustomerProfileAsync(int userId, UpdateCustomerProfileDto dto, CancellationToken cancellationToken = default)
        {
            Customer customer = await _repository.GetByIdAsync(userId, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarningWithContext("Profile update rejected: Customer not found.", new { UserId = userId });
                return GeneralResponse<CustomerProfileDto>.NotFound("Customer account not found.");
            }

            MapCommonFields(customer, dto.FirstName, dto.SecondName, dto.PhoneNumber);
            customer.Address = dto.Address ?? customer.Address;
        
            _repository.Update(customer);
            await _repository.SaveAsync(cancellationToken);

            _logger.LogInfoWithContext("Customer profile details updated successfully.", new { UserId = userId });

            return GeneralResponse<CustomerProfileDto>.Succeeded(MapToProfileDto(customer), "Updated successfully");
        }

        private CustomerProfileDto MapToProfileDto(Customer customer)
        {
            return new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Email!,
                FirstName = customer.FirstName,
                SecondName = customer.SecondName,
                PhoneNumber = customer.PhoneNumber!,
                Role = "Customer",
                AccountStatus = customer.AccountStatus.ToString(),
                Address = customer.Address
            };
        }
    }
}
