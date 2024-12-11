using Application.Models.Users;
using FluentValidation;

namespace Application.Validation.Users;
public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
    }
}
