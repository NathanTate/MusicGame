using Application.Models.Users;
using FluentValidation;

namespace Application.Validation.Users;
public class TokenDtoValidator : AbstractValidator<TokenDto>
{
    public TokenDtoValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
