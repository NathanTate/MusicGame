using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.Common.Helpers;
public static class Validator
{
    public static async Task<ModelStateDictionary> ValidateAsync<T>(IValidator<T> validator, T model, CancellationToken cancellationToken = default)
    {
        ValidationResult result = await validator.ValidateAsync(model, cancellationToken);
        ModelStateDictionary modelStateDictionary = new();

        if (!result.IsValid)
        {
            foreach (ValidationFailure failure in result.Errors)
            {
                modelStateDictionary.AddModelError(
                    failure.PropertyName,
                    failure.ErrorMessage);
            }
        }

        return modelStateDictionary;
    }
}
