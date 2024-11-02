using Application.DTO.Genres;
using FluentValidation;

namespace Application.Validation.Genres;
public class GenreCreateRequestValidator : AbstractValidator<CreateGenreRequest>
{
    public GenreCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 100);
    }
}
