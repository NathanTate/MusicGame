using Application.DTO.Users;
using Domain.Entities;
using FluentResults;
using System.Security.Claims;

namespace Application.Services.Auth;
public interface IAuthenticationService
{
    public Task<Result<string>> RegisterAsync(RegisterRequest model, CancellationToken cancellationToken = default);
    public Task<Result<LoginResponse>> LoginAsync(LoginRequest model, CancellationToken cancellationToken = default);
    public Task<Result> ConfirmEmailAsync(ConfirmEmailRequest model, CancellationToken cancellationToken = default);
    public Task<Result> ResendConfirmationEmailAsync(string Email, CancellationToken cancellationToken = default);
    public Task<Result> ResetPasswordAsync(ResetPasswordRequest model, CancellationToken cancellationToken = default);
    public Task<Result> SendResetPasswordCodeAsync(string Email, CancellationToken cancellationToken = default);
    public Task<Result<TokenDto>> CreateTokenAsync(User user, bool populateExp = true, CancellationToken cancellationToken = default);
    public Task<Result<TokenDto>> RefreshTokenAsync(TokenDto model, CancellationToken cancellationToken = default);
    public Task<User?> FindUserByIdAsync(string userId);
    public Task<User?> FindUserAsync(string email);
}
