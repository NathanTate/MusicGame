namespace Application.Models.Users;
public sealed record TokenDto(string Token, DateTime ExpiresAt);
public sealed record TokenWrapper(TokenDto accessToken, TokenDto refreshToken);
