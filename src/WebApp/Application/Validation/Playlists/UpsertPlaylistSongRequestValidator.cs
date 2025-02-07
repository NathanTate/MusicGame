using Application.Models.Playlists;
using FluentValidation;

namespace Application.Validation.Playlists;
public class UpsertPlaylistSongRequestValidator : AbstractValidator<UpsertSongPlaylistRequest>
{
    public UpsertPlaylistSongRequestValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.SongId).NotEmpty();
        RuleFor(x => x.Position).GreaterThan(0);
    }
}
