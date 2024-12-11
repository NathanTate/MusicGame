namespace Application.Models.Users;
public sealed record ResetPasswordRequest(string Email, string NewPassword, string ConfirmPassword, string ResetCode);
