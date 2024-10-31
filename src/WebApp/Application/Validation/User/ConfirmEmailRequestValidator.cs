using Application.DTO.User;
using FluentValidation;

namespace Application.Validation.User;
public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
    }
}
