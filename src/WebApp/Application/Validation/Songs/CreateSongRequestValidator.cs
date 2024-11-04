using Application.DTO.Songs;
using Domain.Primitives;
using FluentValidation;

namespace Application.Validation.Songs;
public class CreateSongRequestValidator : AbstractValidator<CreateSongRequest>
{
    public CreateSongRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Duration).NotEmpty().GreaterThan(10);
        RuleFor(x => x.ReleaseDate).NotEmpty();
        RuleFor(x => x.SongFile).NotEmpty().Must(x => SD.AllowedSongTypes.Contains(x.ContentType))
            .WithMessage($"File should be of type {string.Join(", ", SD.AllowedSongTypes)}")
            .Must(x => x.Length <= SD.MaxSongSize).WithMessage($"Max song size is: {SD.MaxSongSize / 1024 /1024}mb");

    }
}
