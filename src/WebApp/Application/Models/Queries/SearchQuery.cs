namespace Application.Models.Queries;
public class SearchQuery
{
    private const int _MaxPageSize = 50;
    private int _songsPageSize = 10;
    private int _playlistPageSize = 6;
    private int _usersPageSize = 6;
    private int _genresPageSize = 6;

    public int SongsPageSize
    {
        get => _songsPageSize;
        set => _songsPageSize = value < _MaxPageSize ? value : _MaxPageSize;
    }

    public int PlaylistPageSize
    {
        get => _playlistPageSize;
        set => _playlistPageSize = value < _MaxPageSize ? value : _MaxPageSize;
    }

    public int GenresPageSize
    {
        get => _genresPageSize;
        set => _genresPageSize = value < _MaxPageSize ? value : _MaxPageSize;
    }

    public int UsersPageSize
    {
        get => _usersPageSize;
        set => _usersPageSize = value < _MaxPageSize ? value : _MaxPageSize;
    }
    public int Page { get; set; } = 1;
    public string? SearchTerm { get; set; }
}
