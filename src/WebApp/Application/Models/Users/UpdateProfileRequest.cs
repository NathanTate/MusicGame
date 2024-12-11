using Microsoft.AspNetCore.Http;

namespace Application.Models.Users;

public sealed class UpdateProfileRequest
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public IFormFile? Photo { get; set; }
}

