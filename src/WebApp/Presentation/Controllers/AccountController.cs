using Application.Common.Helpers;
using Application.DTO;
using Application.DTO.User;
using Application.Services.Auth;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Presentation.Controllers;

[Route("account")]
public class AccountController : BaseApiController
{
    private readonly IAuthenticationService _authenticationService;
    public AccountController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model, [FromServices] IValidator<RegisterRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<string> result = await _authenticationService.RegisterAsync(model, HttpContext.RequestAborted);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model, [FromServices] IValidator<LoginRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<LoginResponse> result = await _authenticationService.LoginAsync(model);

        if (result.IsFailed)
            return Unauthorized(result.Errors);

        return Ok(result.Value);
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenDto model, IValidator<TokenDto> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<TokenDto> result = await _authenticationService.RefreshTokenAsync(model);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok(result.Value);
    }

    [HttpPost("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model, IValidator<ConfirmEmailRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.ConfirmEmailAsync(model, HttpContext.RequestAborted);
    
        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("SendResetPasswordCode")]
    public async Task<IActionResult> SendResetPasswordCode([FromBody] EmailRequest model, IValidator<EmailRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.SendResetPasswordCodeAsync(model.Email, HttpContext.RequestAborted);
   
        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok();
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResentPassword([FromBody] ResetPasswordRequest model, IValidator<ResetPasswordRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.ResetPasswordAsync(model, HttpContext.RequestAborted);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("resendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] EmailRequest model, IValidator<EmailRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _authenticationService.ResendConfirmationEmailAsync(model.Email, HttpContext.RequestAborted);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok();
    }
}
