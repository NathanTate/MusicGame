using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Domain.Entities;
using Infrastructure.Interceptors;
using Infrastructure.Context;
using Infrastructure.ExternalProviders;
using Application.InfrastructureInterfaces;
using Microsoft.AspNetCore.Identity;
using Domain.Primitives;
using Infrastructure.Repositories;

namespace Infrastructure;
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

        services.AddAppDbContext(configuration);
        services.AddIdentity();
        services.AddServiceCollections();

        return services;
    }

    private static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        return services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseSqlServer(connectionString)
                .AddInterceptors(new SoftDeleteInterceptor());
        });
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddServiceCollections(this IServiceCollection services)
    {
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
