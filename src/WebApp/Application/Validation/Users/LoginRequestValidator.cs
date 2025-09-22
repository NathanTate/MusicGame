using Application.Models.Users;
using FluentValidation;

namespace Application.Validation.Users;
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().Length(6, 32);
    }
}
