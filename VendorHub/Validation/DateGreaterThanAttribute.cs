using System.ComponentModel.DataAnnotations;

namespace VendorHub.Validation
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var comparisonValue =
                validationContext.ObjectType
                .GetProperty(_comparisonProperty)
                ?.GetValue(validationContext.ObjectInstance);

            if (value == comparisonValue ||
                value is null || comparisonValue is null
                || (DateTime)value < (DateTime)comparisonValue)
                return new ValidationResult(ErrorMessage ?? $"Date must be after {_comparisonProperty}");

            return ValidationResult.Success;
        }
    }
}
