using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Extensions.App.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class MaxFileSizeAttribute : ActionFilterAttribute
{
    private readonly long _maxFileSizeInBytes;

    public MaxFileSizeAttribute(long maxFileSizeInBytes = 10 * 1024 * 1024) // Default to 10MB
    {
        _maxFileSizeInBytes = maxFileSizeInBytes;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;

        if (request.HasFormContentType)
        {
            var files = request.Form.Files;
            if (files.Any(f => f.Length > _maxFileSizeInBytes))
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status413PayloadTooLarge,
                    Title = "Payload Too Large",
                    Detail = $"One or more files exceed the maximum allowed size of {_maxFileSizeInBytes / 1024 / 1024} MB."
                };

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge
                };
                return;
            }
        }
        else if (request.ContentLength.HasValue && request.ContentLength.Value > _maxFileSizeInBytes)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "Payload Too Large",
                Detail = $"The request body exceeds the maximum allowed size of {_maxFileSizeInBytes / 1024 / 1024} MB."
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status413PayloadTooLarge
            };
            return;
        }

        base.OnActionExecuting(context);
    }
}
