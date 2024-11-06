using Application.DTO.Users;
using FluentValidation;

namespace Application.Validation.Users;
public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.NewPassword).NotEmpty().Length(6, 32);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
        RuleFor(x => x.ResetCode).NotEmpty();
    }
}
