using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Extensions
{
    public static class ActionResultExtensions
    {
        public static ActionResult<GeneralResponse<T>> HandleResult<T>(this ControllerBase controller, GeneralResponse<T> response)
        {
            return response.Status switch
            {
                ResultStatus.Success => controller.Ok(response),
                ResultStatus.Created => controller.StatusCode(StatusCodes.Status201Created, response),
                ResultStatus.InvalidInput => controller.BadRequest(response),
                ResultStatus.Unauthenticated => controller.Unauthorized(response),
                ResultStatus.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, response),
                ResultStatus.NotFound => controller.NotFound(response),
                _ => controller.StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }

        public static ActionResult<GeneralResponse> HandleResult(this ControllerBase controller, GeneralResponse response)
        {
            return response.Status switch
            {
                ResultStatus.Success => controller.Ok(response),
                ResultStatus.Created => controller.StatusCode(StatusCodes.Status201Created, response),
                ResultStatus.InvalidInput => controller.BadRequest(response),
                ResultStatus.Unauthenticated => controller.Unauthorized(response),
                ResultStatus.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, response),
                ResultStatus.NotFound => controller.NotFound(response),
                _ => controller.StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}
