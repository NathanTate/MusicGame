namespace Application.Models.Users;
public sealed record ConfirmEmailRequest(string Token, string Email);
