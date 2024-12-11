using Application.Models.Users;
using FluentValidation;

namespace Application.Validation.Users;
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
    }
}
