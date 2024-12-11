using Application.Models.Songs;
using FluentValidation;

namespace Application.Validation.Songs;
public class UpdateSongRequestValidator : AbstractValidator<UpdateSongRequest>
{
    public UpdateSongRequestValidator()
    {
        RuleFor(x => x.SongId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Duration).NotEmpty().GreaterThan(10);
        RuleFor(x => x.ReleaseDate).NotEmpty();
        RuleFor(x => x.GenreIds).NotEmpty();
    }
}
