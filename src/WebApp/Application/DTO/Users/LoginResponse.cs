﻿using Domain.Entities;

namespace Application.DTO.Users;

public sealed class LoginResponse
{
    public LoginResponse(User user, TokenDto tokens)
    {
        UserId = user.Id;
        Email = user.Email!;
        Username = user.DisplayName;
        Roles = user.Roles;
        Tokens = tokens;
    }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public TokenDto Tokens { get; set; }
}
