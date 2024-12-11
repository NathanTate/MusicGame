using Application.Models.Users;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Route("api/users")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    public UserController(UserManager<User> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUser(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return NotFound($"User with email {email} cannot be found");
        }

        return Ok(_mapper.Map<UserResponse>(user));
    }

    [HttpGet("byId/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound($"User with id {userId} cannot be found");
        }

        return Ok(_mapper.Map<UserResponse>(user));
    }
}
