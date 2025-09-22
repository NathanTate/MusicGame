using Application.Interfaces;
using Application.Models.Queries;
using Application.Models.Users;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Route("api/users")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUser(string email)
    {
        Result<UserResponse> result = await _userService.GetUserByEmailAsync(email);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpGet("byId/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        Result<UserResponse> result = await _userService.GetUserAsync(userId);

        return result.ToHttpResponse(HttpContext);
    }

    [AllowAnonymous]
    [HttpGet("{userId}/playlists")]
    public async Task<IActionResult> GetUserPlaylists(string userId, [FromQuery] UserPlaylistsQuery query)
    {
        Result<UserResponse> result = await _userService.GetUserAsync(userId);

        if (result.IsFailed)
        {
            return result.ToHttpResponse(HttpContext);
        }

        var isOwn = User?.GetUserId() == userId;
        return Ok(await _userService.GetUserPlaylists(query, userId, isOwn, HttpContext.RequestAborted));
    }

    [AllowAnonymous]
    [HttpGet("{userId}/playlists/liked")]
    public async Task<IActionResult> GetLikedPlaylists(string userId, [FromQuery] UserPlaylistsQuery query)
    {
        Result<UserResponse> result = await _userService.GetUserAsync(userId);

        if (result.IsFailed)
        {
            return result.ToHttpResponse(HttpContext);
        }

        return Ok(await _userService.GetLikedPlaylists(query, userId, HttpContext.RequestAborted));
    }

    [AllowAnonymous]
    [HttpGet("{userId}/songs")]
    public async Task<IActionResult> GetUserSongs(string userId, [FromQuery] UserSongsQuery query)
    {
        Result<UserResponse> result = await _userService.GetUserAsync(userId);

        if (result.IsFailed)
        {
            return result.ToHttpResponse(HttpContext);
        }

        var isOwn = User?.GetUserId() == userId;
        return Ok(await _userService.GetUsersSongs(query, userId, isOwn, HttpContext.RequestAborted));
    }

    [AllowAnonymous]
    [HttpGet("{userId}/songs/liked")]
    public async Task<IActionResult> GetLikedSongs(string userId, [FromQuery] UserSongsQuery query)
    {
        Result<UserResponse> result = await _userService.GetUserAsync(userId);

        if (result.IsFailed)
        {
            return result.ToHttpResponse(HttpContext);
        }

        return Ok(await _userService.GetLikedSongs(query, userId, HttpContext.RequestAborted));
    }

    [AllowAnonymous]
    [HttpGet("{userId}/songs/popular")]
    public async Task<IActionResult> GetMostPopularSongs(string userId)
    {
        Result<UserResponse> result = await _userService.GetUserAsync(userId);

        if (result.IsFailed)
        {
            return result.ToHttpResponse(HttpContext);
        }

        return Ok(await _userService.GetMostPopularSongs(userId, HttpContext.RequestAborted));
    }
}
