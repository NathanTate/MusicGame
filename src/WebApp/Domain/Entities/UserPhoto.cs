namespace Domain.Entities;
public class UserPhoto : Photo
{
    public int UserPhotoId { get; set; }
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
}
