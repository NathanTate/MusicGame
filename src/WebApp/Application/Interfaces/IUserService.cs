using Application.DTO.Users;
using FluentResults;

namespace Application.Interfaces;
public interface IUserService
{
    Task<Result<UserResponse>> GetUserAsync(string email);
    Task<Result<UserResponse>> UpdateUserProfileAsync(UpdateProfileRequest model, string userId, CancellationToken cancellationToken = default); 
}
