namespace Application.DTO.Users;
public sealed record ConfirmEmailRequest(string Token, string Email);
