namespace Application.DTO.User;
public sealed record ResetPasswordRequest(string Email, string NewPassword, string ConfirmPassword, string ResetCode);
