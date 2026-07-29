using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class BaseUserService<T> where T : User
    {
        protected readonly IGeneralRepository<T> _repository;

        protected BaseUserService(IGeneralRepository<T> repository)
        {
            _repository = repository;
        }

        protected void MapCommonFields(T existingUser, string? firstName, string? secondName, string? phoneNumber)
        {
            existingUser.FirstName = firstName ?? existingUser.FirstName;
            existingUser.SecondName = secondName ?? existingUser.SecondName;
            existingUser.PhoneNumber = phoneNumber ?? existingUser.PhoneNumber;
        }
    }
}
