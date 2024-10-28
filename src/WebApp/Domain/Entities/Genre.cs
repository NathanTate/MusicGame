namespace Domain.Entities;
public class Genre
{
    public int GenreId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsSystemDefined { get; set; }

    public List<Song> Songs { get; } = [];
}
