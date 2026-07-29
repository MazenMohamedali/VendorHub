using Microsoft.AspNetCore.Mvc;

namespace VendorHub.Exceptions
{
    internal sealed class GlobalExceptionHandlerMiddleWare(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleWare> logger)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            } catch(Exception ex)
            {
                logger.LogError(ex, "Unhandled Exception occured");

                context.Response.StatusCode = ex switch
                {
                    ApplicationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Type = ex.GetType().Name,
                        Title = "An error Occured", 
                        Detail = ex.Message
                    }
                );
            }
        }
    }
}
