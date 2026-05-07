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
            if(!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(
                    new GeneralResponse
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ValidationService.GetValidationErrors(context.ModelState)
                    });
            }
            base.OnActionExecuting(context);
        }
    }
}
