using Application.Common.Helpers;
using Application.Models;
using Application.Models.Users;
using Application.Interfaces;
using Application.Services.Auth;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Route("account")]
public class AccountController : BaseApiController
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserService _userService;
    public AccountController(IAuthenticationService authenticationService, IUserService userService)
    {
        _authenticationService = authenticationService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model, [FromServices] IValidator<RegisterRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<string> result = await _authenticationService.RegisterAsync(model, HttpContext.RequestAborted);

        return result.ToCreateHttpResponse(HttpContext);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model, [FromServices] IValidator<LoginRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<LoginResponse> result = await _authenticationService.LoginAsync(model);

        if (result.IsSuccess)
            SetCookieToken(result.Value.Tokens.accessToken);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenDto refreshToken, [FromServices] IValidator<TokenDto> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, refreshToken, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<TokenWrapper> result = await _authenticationService.RefreshTokenAsync(refreshToken);

        if (result.IsSuccess)
            SetCookieToken(result.Value.accessToken);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model, [FromServices] IValidator<ConfirmEmailRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.ConfirmEmailAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("SendResetPasswordCode")]
    public async Task<IActionResult> SendResetPasswordCode([FromBody] EmailRequest model, [FromServices] IValidator<EmailRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.SendResetPasswordCodeAsync(model.Email, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResentPassword([FromBody] ResetPasswordRequest model, [FromServices] IValidator<ResetPasswordRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.ResetPasswordAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("resendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] EmailRequest model, [FromServices] IValidator<EmailRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.ResendConfirmationEmailAsync(model.Email, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateAccount(UpdateProfileRequest model, [FromServices] IValidator<UpdateProfileRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<UserResponse> result = await _userService.UpdateUserProfileAsync(model, User.GetUserId()!, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    private void SetCookieToken(TokenDto accessToken)
    {
        HttpContext.Response.Cookies.Append("accessToken", accessToken.Token,
        new CookieOptions
        {
            HttpOnly = true,
            Expires = accessToken.ExpiresAt,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        });
    }
}
