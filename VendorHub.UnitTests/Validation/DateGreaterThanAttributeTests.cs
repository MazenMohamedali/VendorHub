using FluentAssertions;
using System;
using System.ComponentModel.DataAnnotations;
using VendorHub.Validation;

namespace VendorHub.UnitTests.Validation
{
    public class DateGreaterThanAttributeTests
    {
        private class TestModel
        {
            public DateTime? ProductionDate { get; set; }
            public DateTime? ExpireDate { get; set; }
        }

        private static ValidationResult? Validate(TestModel model, string comparisonProperty = nameof(TestModel.ProductionDate))
        {
            var attribute = new DateGreaterThanAttribute(comparisonProperty);
            var context = new ValidationContext(model)
            {
                MemberName = nameof(TestModel.ExpireDate)
            };

            return attribute.GetValidationResult(model.ExpireDate, context);
        }

        [Fact]
        public void IsValid_WhenExpireDateIsLaterThanProductionDate_ReturnsSuccess()
        {
            // Arrange
            var model = new TestModel
            {
                ProductionDate = new DateTime(2026, 1, 1),
                ExpireDate = new DateTime(2026, 6, 1)
            };

            // Act
            var result = Validate(model);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValid_WhenExpireDateIsEarlierThanProductionDate_ReturnsValidationError()
        {
            // Arrange
            var model = new TestModel
            {
                ProductionDate = new DateTime(2026, 6, 1),
                ExpireDate = new DateTime(2026, 1, 1)
            };

            // Act
            var result = Validate(model);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBe(ValidationResult.Success);
            result!.ErrorMessage.Should().Contain("Date must be after ProductionDate");
        }

        [Fact]
        public void IsValid_WhenExpireDateIsEqualToProductionDate_ReturnsValidationError()
        {
            // Arrange
            var sameDate = new DateTime(2026, 3, 15);
            var model = new TestModel
            {
                ProductionDate = sameDate,
                ExpireDate = sameDate
            };

            // Act
            var result = Validate(model);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBe(ValidationResult.Success);
        }

        [Fact]
        public void IsValid_WhenExpireDateIsNull_ReturnsSuccess()
        {
            // Arrange
            var model = new TestModel
            {
                ProductionDate = new DateTime(2026, 1, 1),
                ExpireDate = null
            };

            // Act
            var result = Validate(model);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValid_WhenProductionDateIsNull_ReturnsSuccess()
        {
            // Arrange
            var model = new TestModel
            {
                ProductionDate = null,
                ExpireDate = new DateTime(2026, 6, 1)
            };

            // Act
            var result = Validate(model);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }
    }
}
