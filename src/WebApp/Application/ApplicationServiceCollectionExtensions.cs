using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Domain.Primitives;
using Application.Services.Auth;
using Application.Interfaces;
using Application.Services;
using Application.Common.UserHelpers;
using Application.Authorization;

namespace Application;
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        var jwtOptions = new JwtOptions();
        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
        configuration.Bind("JwtOptions", jwtOptions);

        services.AddHttpContextAccessor();
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddAuthentication(jwtOptions);
        services.AddServiceCollections();

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, JwtOptions jwtOptions)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddServiceCollections(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<ISongService, SongService>();
        services.AddScoped<IPlaylistService, PlaylistService>();
        
        services.AddScoped<ISongAuthorizationService, SongAuthorizationService>();
        services.AddScoped<IPlaylistAuthorizationService, PlaylistAuthorizationService>();

        return services;
    }
}
