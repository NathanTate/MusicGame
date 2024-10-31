using Application.DTO;
using FluentValidation;

namespace Application.Validation;
public class EmailRequestValidator : AbstractValidator<EmailRequest>
{
    public EmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
    }
}
