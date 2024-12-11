﻿using Application.Models.Users;
using Application.Errors;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Application.Services;

internal class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly UserManager<User> _userManager;
    public UserService(AppDbContext dbContext, IMapper mapper, IFileHandler fileHandler, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _userManager = userManager;
    }
    public async Task<Result<UserResponse>> GetUserAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return new NotFoundError($"User with email {email} cannot be found");
        }

        return Result.Ok(_mapper.Map<UserResponse>(user));
    }

    public async Task<Result<UserResponse>> UpdateUserProfileAsync(UpdateProfileRequest model, string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return new NotFoundError($"User with id {userId} cannot be found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        user.Roles = roles.ToList();
        user.Email = model.Email;
        user.DisplayName = model.Username;

        if (model.Photo is not null)
        {
            var photoUrl = user.Photo is null
            ? await _fileHandler.UploadFileAsync(model.Photo, FileContainer.Photos, cancellationToken)
            : await _fileHandler.UpdateFileAsync(Path.GetFileName(user.Photo.URL), model.Photo, FileContainer.Photos, cancellationToken);

            var userPhoto = new Photo()
            {
                URL = photoUrl,
                Size = model.Photo.Length,
                ContentType = model.Photo.ContentType,
            };

            user.Photo = userPhoto;
            await _dbContext.SaveChangesAsync();
        }

        return Result.Ok(_mapper.Map<UserResponse>(user));
    }
}
