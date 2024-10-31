using Application.DTO.User;
using FluentValidation;

namespace Application.Validation.User;
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().Length(6, 32);
    }
}
