namespace Application.Models.Users;
public sealed class UserResponse
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Roles { get; set; } = [];
    public PhotoResponse? Photo { get; set; }
}
