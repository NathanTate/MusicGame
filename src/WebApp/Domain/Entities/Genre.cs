using Domain.Primitives;

namespace Domain.Entities;
public class Genre : ISoftDeletable
{
    public int GenreId { get; set; }
    public string Name { get; set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public bool IsSystemDefined { get; set; }
    public bool isDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public List<Song> Songs { get; } = [];
}
