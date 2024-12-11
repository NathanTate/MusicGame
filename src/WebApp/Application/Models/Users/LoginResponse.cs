﻿using Domain.Entities;

namespace Application.Models.Users;

public sealed class LoginResponse
{
    public LoginResponse(User user, TokenWrapper tokens)
    {
        UserId = user.Id;
        Email = user.Email!;
        Username = user.DisplayName;
        Roles = user.Roles;
        ProfilePhotoUrl = user.Photo?.URL;
        Tokens = tokens;
    }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public TokenWrapper Tokens { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}