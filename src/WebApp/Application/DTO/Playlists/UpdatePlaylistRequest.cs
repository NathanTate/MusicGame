namespace Application.DTO.Playlists;
public sealed record UpdatePlaylistRequest(string Name, string? Description, bool IsPrivate);
