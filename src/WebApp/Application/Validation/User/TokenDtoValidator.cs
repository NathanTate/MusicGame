using Application.DTO.User;
using FluentValidation;

namespace Application.Validation.User;
public class TokenDtoValidator : AbstractValidator<TokenDto>
{
    public TokenDtoValidator()
    {
        RuleFor(x => x.AccessToken).SetValidator(new TokenBaseValidator());
        RuleFor(x => x.RefreshToken).SetValidator(new TokenBaseValidator());
    }
}

public class TokenBaseValidator : AbstractValidator<TokenBase>
{
    public TokenBaseValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
