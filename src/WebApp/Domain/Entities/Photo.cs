using Domain.Primitives;

namespace Domain.Entities;
public abstract class Photo : ISoftDeletable
{
    public string URL { get; set; } = null!;
    public long Size { get; set; }
    public string ContentType { get; set; } = null!;
    public bool isDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

}
