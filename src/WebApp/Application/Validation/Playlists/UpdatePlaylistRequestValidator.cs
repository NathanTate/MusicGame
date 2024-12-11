using Application.Models.Playlists;
using FluentValidation;

namespace Application.Validation.Playlists;
public class UpdatePlaylistRequestValidator : AbstractValidator<UpdatePlaylistRequest>
{
    public UpdatePlaylistRequestValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Description).MinimumLength(5).MaximumLength(300);
        RuleFor(x => x.IsPrivate).NotEmpty();
    }
}
