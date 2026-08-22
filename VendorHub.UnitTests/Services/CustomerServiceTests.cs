using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VendorHub.DTOs.CustomerDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IGeneralRepository<Customer>> _customerRepositoryMock;
        private readonly Mock<ILogger<CustomerService>> _loggerMock;

        public CustomerServiceTests()
        {
            _customerRepositoryMock = new Mock<IGeneralRepository<Customer>>();
            _loggerMock = new Mock<ILogger<CustomerService>>();
        }

        private CustomerService CreateSut()
        {
            return new CustomerService(_customerRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetCustomerProfileAsync_WhenCustomerNotFound_ReturnsNotFound()
        {
            // Arrange
            const int userId = 99;

            _customerRepositoryMock
                .Setup(r => r.GetByAsNoTracking(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(new List<Customer>().BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetCustomerProfileAsync(userId);

            // Assert
            result.ShouldBeNotFound();
        }

        [Fact]
        public async Task GetCustomerProfileAsync_WhenCustomerExists_ReturnsProfileDto()
        {
            // Arrange
            const int userId = 1;
            var customer = new Customer
            {
                Id = userId,
                Email = "customer@example.com",
                FirstName = "Jane",
                SecondName = "Doe",
                PhoneNumber = "01000000000",
                AccountStatus = AccountStatus.ACTIVE,
                Address = "123 Main St"
            };

            _customerRepositoryMock
                .Setup(r => r.GetByAsNoTracking(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(new List<Customer> { customer }.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetCustomerProfileAsync(userId);

            // Assert
            var data = result.ShouldBeSucceeded();
            data.Id.Should().Be(userId);
            data.Email.Should().Be("customer@example.com");
            data.FirstName.Should().Be("Jane");
            data.SecondName.Should().Be("Doe");
            data.Address.Should().Be("123 Main St");
            data.Role.Should().Be("Customer");
            data.AccountStatus.Should().Be("ACTIVE");
        }

        [Fact]
        public async Task UpdateCustomerProfileAsync_WhenCustomerNotFound_ReturnsNotFound()
        {
            // Arrange
            const int userId = 99;
            var dto = new UpdateCustomerProfileDto
            {
                FirstName = "NewFirst",
                SecondName = "NewSecond",
                PhoneNumber = "01111111111",
                Address = "New Address"
            };

            _customerRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateCustomerProfileAsync(userId, dto);

            // Assert
            result.ShouldBeNotFound();
            _customerRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task UpdateCustomerProfileAsync_WhenValid_UpdatesFieldsAndReturnsSucceeded()
        {
            // Arrange
            const int userId = 1;
            var existingCustomer = new Customer
            {
                Id = userId,
                Email = "customer@example.com",
                FirstName = "OldFirst",
                SecondName = "OldSecond",
                PhoneNumber = "01000000000",
                Address = "Old Address",
                AccountStatus = AccountStatus.ACTIVE
            };

            var dto = new UpdateCustomerProfileDto
            {
                FirstName = "UpdatedFirst",
                SecondName = "UpdatedSecond",
                PhoneNumber = "01122334455",
                Address = "Updated Address"
            };

            _customerRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateCustomerProfileAsync(userId, dto);

            // Assert
            var data = result.ShouldBeSucceeded();
            data.FirstName.Should().Be("UpdatedFirst");
            data.SecondName.Should().Be("UpdatedSecond");
            data.PhoneNumber.Should().Be("01122334455");
            data.Address.Should().Be("Updated Address");

            existingCustomer.FirstName.Should().Be("UpdatedFirst");
            existingCustomer.SecondName.Should().Be("UpdatedSecond");
            existingCustomer.Address.Should().Be("Updated Address");

            _customerRepositoryMock.Verify(r => r.Update(existingCustomer), Times.Once);
            _customerRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
