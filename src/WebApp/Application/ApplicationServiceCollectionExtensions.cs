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
using Elastic.Clients.Elasticsearch;
using Application.Services.Elastic;
using GeniusLyrics.NET;

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
        var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .DisableDirectStreaming();
        services.AddSingleton(new ElasticsearchClient(settings));
        services.AddSingleton(new GeniusClient(configuration.GetValue<string>("GeniusApiKey") ?? ""));
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

                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.HttpContext.Request.Cookies.TryGetValue("accessToken", out string? accessToken);
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }
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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<SongsElasticService>();
        services.AddScoped<PlaylistsElasticService>();
        services.AddScoped<UsersElasticService>();
        services.AddScoped<GenresElasticService>();

        return services;
    }
}
