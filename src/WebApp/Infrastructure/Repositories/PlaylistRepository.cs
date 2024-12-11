//using Application.InfrastructureInterfaces;
//using Domain.Entities;
//using Infrastructure.Context;
//using Microsoft.EntityFrameworkCore;

//namespace Infrastructure.Repositories;
//internal class PlaylistRepository : IPlaylistRepository
//{
//    private readonly AppDbContext _dbContext;
//    public PlaylistRepository(AppDbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    public void Create(Playlist playlist)
//    {
//        _dbContext.Add(playlist);
//    }

//    public Task<List<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
//    {
//        return _dbContext.Playlists
//            .AsNoTracking()
//            .Where(x => !x.IsPrivate)
//            .Include(x => x.Songs)
//                .ThenInclude(s => s.Song)
//            .ToListAsync(cancellationToken);
//    }

//    public async Task<Playlist?> GetByIdAsync(int playlistId, bool tracking = true, string? userId = null, CancellationToken cancellationToken = default)
//    {
//        var query = _dbContext.Playlists
//            .Where(x => x.PlaylistId == playlistId && (!x.IsPrivate || x.UserId == userId))
//            .Include(x => x.Photo)
//            .Include(x => x.Songs.OrderBy(x => x.Position))
//                .ThenInclude(s => s.Song)
//                .ThenInclude(s => s.Photo)
//            .Include(x => x.Songs.OrderBy(x => x.Position))
//                .ThenInclude(s => s.Song)
//                .ThenInclude(s => s.User)
//            .Include(x => x.User)
//            .AsSplitQuery();

//        if (!tracking)
//        {
//            query.AsNoTracking();
//        }


//        return await query.FirstOrDefaultAsync(cancellationToken);
//    }

//    public void Update(Playlist playlist)
//    {
//        _dbContext.Playlists.Update(playlist);
//    }

//    public void Delete(Playlist playlist)
//    {
//        _dbContext.Entry(playlist).State = EntityState.Deleted;
//    }

//    public void AddSongToPlaylist(int songId, int playlistId, int position = 0)
//    {
//        var playlistSong = new PlaylistSong()
//        {
//            PlaylistId = playlistId,
//            SongId = songId,
//            Position = position
//        };

//        _dbContext.PlaylistSongs.Add(playlistSong);
//    }

//    public async Task<PlaylistSong?> GetPlaylistSongAsync(int songId, int playlistId, bool tracking = true, CancellationToken cancellationToken = default)
//    {
//        var query = _dbContext.PlaylistSongs
//                    .Include(x => x.Playlist)
//                    .Select(x => new PlaylistSong
//                    {
//                        PlaylistId = x.PlaylistId,
//                        SongId = x.SongId,
//                        Position = x.Position,
//                        Playlist = x.Playlist
//                    });

//        if (!tracking)
//        {
//            query.AsNoTracking();
//        }

//        return await query.FirstOrDefaultAsync(x => x.SongId == songId && x.PlaylistId == playlistId, cancellationToken);
//    }

//    public void RemoveSongFromPlaylist(PlaylistSong playlistSong)
//    {
//        _dbContext.Entry(playlistSong).State = EntityState.Deleted;
//    }
//    public void UpdateSongPosition(PlaylistSong playlistSong)
//    {
//        _dbContext.PlaylistSongs.Update(playlistSong);
//    }
//}
