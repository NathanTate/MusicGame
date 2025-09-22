namespace Application.Models.Playlists;
public sealed record UpdatePlaylistRequest(int PlaylistId, string Name, string? Description, bool IsPrivate);
