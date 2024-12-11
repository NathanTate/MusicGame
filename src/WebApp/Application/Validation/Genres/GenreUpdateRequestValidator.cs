using Application.Models.Genres;
using FluentValidation;

namespace Application.Validation.Genres;
public class GenreUpdateRequestValidator : AbstractValidator<UpdateGenreRequest>
{
    public GenreUpdateRequestValidator()
    {
        RuleFor(x => x.GenreId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().Length(3, 100);
    }
}
