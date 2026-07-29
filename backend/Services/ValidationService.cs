using Microsoft.AspNetCore.Mvc.ModelBinding;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public class ValidationService
    {
        public static IEnumerable<ValidationError> GetValidationErrors(ModelStateDictionary modelState)
        {
            List<ValidationError> errors = new();

            foreach (var modelState_Key in modelState.Keys)
            {
                var value = modelState[modelState_Key];
                if (value.Errors.Count > 0)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(new ValidationError
                        {
                            Field = modelState_Key,
                            Message = error.ErrorMessage
                        });
                    }
                }
            }

            return errors;
        }
    }
}
