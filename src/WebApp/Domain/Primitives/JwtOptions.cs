namespace Domain.Primitives;
public class JwtOptions
{
    public string SecurityKey { get; set; } = string.Empty;
    public double AccessTokenExpiresInMinutes { get; set; } = 5;
    public double RefreshTokenExpiresInDays { get; set; } = 7;
}
