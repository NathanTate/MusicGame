using Application.DTO.User;
using Application.InfrastructureInterfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Primitives;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.Auth;
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtOptions _jwtOptions;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthenticationService> _logger;
    public AuthenticationService(UserManager<User> userManager, ILogger<AuthenticationService> logger, IOptions<JwtOptions> jwtOptions, IEmailSender emailSender)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result<string>> RegisterAsync(RegisterRequest model, CancellationToken cancellationToken = default)
    {
        var user = new User()
        {
            Email = model.Email,
            UserName = model.Email,
            DisplayName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return Result.Fail(result.Errors.Select(err => err.Description));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.USER));
        if (!roleResult.Succeeded)
        {
            return Result.Fail(roleResult.Errors.Select(err => err.Description));
        }

        await SendConfirmationEmailAsync(user, cancellationToken);

        return user.Id;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.ToUpper());
        
        if (user is null)
        { 
            return Result.Fail("Invalid Email or Password"); 
        }

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Result.Fail("Invalid Email or Password");
        }
           
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Result.Fail("Email confirmation needed");
        }    
        
        Result<TokenDto> result = await CreateTokenAsync(user, populateExp: true, cancellationToken);

        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        var loginResponse = new LoginResponse(user, result.Value);

        return loginResponse;
    }

    public async Task<Result<TokenDto>> CreateTokenAsync(User user, bool populateExp = true, CancellationToken cancellationToken = default)
    {
        user.Roles = [.. (await _userManager.GetRolesAsync(user))];

        TokenBase accessToken = GenerateToken(user);
        string refreshTokenString = GenerateRefreshToken();
        var refreshTokenExpiresAt = populateExp 
            ? DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiresInDays) 
            : user.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiresInDays);


        user.RefreshToken = refreshTokenString;
        user.RefreshTokenExpiryTime = refreshTokenExpiresAt;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return Result.Fail(result.Errors.Select(err => err.Description));
        }

        var refreshToken = new TokenBase(refreshTokenString, refreshTokenExpiresAt);

        return new TokenDto(accessToken, refreshToken);
    }

    public async Task<Result<TokenDto>> RefreshTokenAsync(TokenDto model, CancellationToken cancellationToken = default)
    {
        TokenBase accessToken = model.AccessToken;
        TokenBase refreshToken = model.RefreshToken;

        Result<ClaimsIdentity> result = await GetIdentityFromExpiredTokenAsync(accessToken.Token);

        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        var claimsIdentity = result.Value;
        var user = await _userManager.GetUserAsync(new ClaimsPrincipal(claimsIdentity));

        if (isRefreshTokenInvalid())
        {
            return Result.Fail("Refresh token is invalid or expired");
        }

        return await CreateTokenAsync(user!, populateExp: false, cancellationToken);

        bool isRefreshTokenInvalid() => user is null || user.RefreshToken != refreshToken.Token 
            || user.RefreshTokenExpiryTime < DateTime.UtcNow;
    }

    public async Task<Result<ClaimsIdentity>> GetIdentityFromExpiredTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        var tokenHanlder = new JwtSecurityTokenHandler();

        TokenValidationResult validationResult = await tokenHanlder.ValidateTokenAsync(accessToken, tokenValidationParameters);

        if (!validationResult.IsValid)
        {
            return Result.Fail("Invalid Token");
        }

        var jwtSecurityToken = validationResult.SecurityToken as JwtSecurityToken;

        if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            return Result.Fail("Invalid Token");
        }

        return validationResult.ClaimsIdentity;

    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.ToUpper());

        if (user is null)
        {
            return Result.Fail("User doesn't exist");
        }

        var result = await _userManager.ConfirmEmailAsync(user, model.Token);

        if (!result.Succeeded)
        {
            return Result.Fail(result.Errors.Select(err => err.Description));
        }

        return Result.Ok();
    }

    public async Task<Result> ResendConfirmationEmailAsync(string Email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(Email.ToUpper());

        if (user is null)
        {
            return Result.Fail("User doesn't exist");
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            return Result.Fail("Email is already confirmed");
        }

        await SendConfirmationEmailAsync(user, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.ToUpper());

        if (user is null)
        {
            return Result.Fail("User doesn't exist");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.ResetCode, model.NewPassword);

        if (!result.Succeeded)
        {
            return Result.Fail(result.Errors.Select(err => err.Description));
        }

        return Result.Ok();
    }

    public async Task<Result> SendResetPasswordCodeAsync(string Email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(Email.ToUpper());

        if (user is null)
        {
            return Result.Fail("User doesn't exist");
        }

        var resetPasswordCode = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _emailSender.SendAsync(
            user.Email, 
            "Password Reset", 
            $"Enter the code to proceed: {resetPasswordCode}", 
            cancellationToken: cancellationToken);

        return Result.Ok();
    }

    private TokenBase GenerateToken(User user)
    {
        var _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiresInMinutes);

        var serucityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512),
            Expires = ExpiresAt
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken token = tokenHandler.CreateToken(serucityTokenDescriptor);

        TokenBase accessToken = new TokenBase(tokenHandler.WriteToken(token), ExpiresAt);

        return accessToken;
    }

    private string GenerateRefreshToken()
    {
        var bytesBuffer = new byte[48];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytesBuffer);
            return Convert.ToBase64String(bytesBuffer);
        }
    }

    private async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var emailVerificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var url = $"Click this link for verification https://localhost:7221/account/verifyEmail?token={emailVerificationToken}&email={user.Email}";
        
        await _emailSender.SendAsync(
            user.Email, 
            "Email Verification", 
            url,
            cancellationToken: cancellationToken);
    }

}
