using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartQ.API.Filters;

public class InvalidOperationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidOperationException ex)
        {
            context.Result = new BadRequestObjectResult(new { message = ex.Message });
            context.ExceptionHandled = true;
        }
    }
}
