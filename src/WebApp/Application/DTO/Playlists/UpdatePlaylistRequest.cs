namespace Application.DTO.Playlists;
public sealed record UpdatePlaylistRequest(int PlaylistId, string Name, string? Description, bool IsPrivate);
