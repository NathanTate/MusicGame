using Application.Errors;
using FluentResults;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToHttpResponse<T>(this Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            if (result.ValueOrDefault is null)
            {
                return new NoContentResult();
            }
            return new OkObjectResult(result.Value);
        }

        return HandleError(result, httpContext);
    }

    public static IActionResult ToHttpResponse(this Result result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return HandleError(result, httpContext);
    }

    public static IActionResult ToCreateHttpResponse<T>(this Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result.Value)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        return HandleError(result, httpContext);
    }

    private static ObjectResult HandleError<T>(Result<T> result, HttpContext httpContext)
    {
        var firstError = result.Errors.FirstOrDefault();

        if (firstError is ValidationError)
        {
            return HandleValidationError(result, httpContext);
        }
        else if (firstError is NotFoundError)
        {
            return HandleNotFoundError(result, httpContext);
        }
        else if (firstError is ForbiddenAccessError)
        {
            return HandleForbiddenError(result, httpContext);
        }

        return new BadRequestObjectResult(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Unexpected Error",
            Detail = result.Errors.FirstOrDefault()?.Message ?? string.Empty,
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        });
    }

     private static ObjectResult HandleError(Result result, HttpContext httpContext)
    {
        var firstError = result.Errors.FirstOrDefault();

        if (firstError is ValidationError)
        {
            return HandleValidationError(result, httpContext);
        }
        else if (firstError is NotFoundError)
        {
            return HandleNotFoundError(result, httpContext);
        }
        else if (firstError is ForbiddenAccessError)
        {
            return HandleForbiddenError(result, httpContext);
        }

        return new BadRequestObjectResult(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Unexpected Error",
            Detail = result.Errors.FirstOrDefault()?.Message ?? string.Empty,
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        });
    }

    private static ObjectResult HandleValidationError<T>(Result<T> result, HttpContext httpContext)
    {
        var details = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "ValidationError", result.Errors.Select(c => c.Message).ToArray() }
        })
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1\"",
            Title = "Validation Error",
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        };

        return new BadRequestObjectResult(details);
    }

    private static ObjectResult HandleValidationError(Result result, HttpContext httpContext)
    {
        var details = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "ValidationError", result.Errors.Select(c => c.Message).ToArray() }
        })
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1\"",
            Title = "Validation Error",
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        };

        return new BadRequestObjectResult(details);
    }

    private static ObjectResult HandleNotFoundError<T>(Result<T> result, HttpContext httpContext)
    {
        var details = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found",
            Detail = result.Errors.FirstOrDefault()?.Message ?? string.Empty,
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        };

        return new NotFoundObjectResult(details);
    }

    private static ObjectResult HandleNotFoundError(Result result, HttpContext httpContext)
    {
        var details = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found",
            Detail = result.Errors.FirstOrDefault()?.Message ?? string.Empty,
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"

        };

        return new NotFoundObjectResult(details);
    }

    private static ObjectResult HandleForbiddenError<T>(Result<T> result, HttpContext httpContext)
    {
        var details = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden",
            Detail = result.Errors.FirstOrDefault()?.Message ?? string.Empty,
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        };

        return new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private static ObjectResult HandleForbiddenError(Result result, HttpContext httpContext)
    {
        var details = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden",
            Detail = result.Errors.FirstOrDefault()?.Message ?? string.Empty,
            Instance = $"{httpContext?.Request.Method} {httpContext?.Request.GetDisplayUrl()}"
        };

        return new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
