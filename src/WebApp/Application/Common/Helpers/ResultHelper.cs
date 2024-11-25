using Application.Errors;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace Application.Common.Helpers;

public static class ResultHelper
{
    public static Result<T> ErrorsToResult<T>(IEnumerable<IdentityError> errors)
    {
        return Result.Fail(errors.Select(x => new ValidationError(x.Description)));
    }

    public static Result ErrorsToResult(IEnumerable<IdentityError> errors)
    {
        return Result.Fail(errors.Select(x => new ValidationError(x.Description)));
    }

    public static Result<T> ErrorsToResult<T>(IEnumerable<IError> errors)
    {
        return Result.Fail(errors.Select(x => new ValidationError(x.Message)));
    }

    public static Result ErrorsToResult(IEnumerable<IError> errors)
    {
        return Result.Fail(errors.Select(x => new ValidationError(x.Message)));
    }

    public static Result<T> ErrorsToResult<T>(IEnumerable<string> errors)
    {
        return Result.Fail(errors.Select(err => new ValidationError(err)));
    }

    public static Result ErrorsToResult(IEnumerable<string> errors)
    {
        return Result.Fail(errors.Select(err => new ValidationError(err)));
    }
}
