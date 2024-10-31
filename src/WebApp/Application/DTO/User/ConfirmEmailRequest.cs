namespace Application.DTO.User;
public sealed record ConfirmEmailRequest(string Token, string Email);
