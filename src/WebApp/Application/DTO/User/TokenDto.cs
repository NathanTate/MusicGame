namespace Application.DTO.User;
public sealed record TokenDto(TokenBase AccessToken, TokenBase RefreshToken);

public sealed record TokenBase(string Token, DateTime ExpiresAt);


