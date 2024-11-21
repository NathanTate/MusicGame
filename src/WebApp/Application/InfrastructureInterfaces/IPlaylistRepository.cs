using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface IPlaylistRepository
{
    Task<List<Playlist>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Playlist?> GetByIdAsync(int playlistId, bool tracking = true, string? userId = null, CancellationToken cancellationToken = default);
    void Create(Playlist playlist);
    void Update(Playlist playlist);
    void Delete(Playlist playlist);
    Task<PlaylistSong?> GetPlaylistSongAsync(int songId, int playlistId, bool tracking = true, CancellationToken cancellationToken = default);
    void AddSongToPlaylist(int songId, int playlistId, int position = default);
    void RemoveSongFromPlaylist(PlaylistSong playlistSong);
    void UpdateSongPosition(PlaylistSong playlistSong);
}
