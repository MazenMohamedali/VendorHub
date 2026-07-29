using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VendorHub.DTOs.sharedDto;
using VendorHub.Services;

namespace VendorHub.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = ValidationService.GetValidationErrors(context.ModelState);
                context.Result = new BadRequestObjectResult(GeneralResponse.InvalidInput("One or more validation errors occurred.", errors));
            }
            base.OnActionExecuting(context);
        }
    }
}
