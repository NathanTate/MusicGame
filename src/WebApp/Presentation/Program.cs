using Application;
using Application.Services.Elastic;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Context;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;
using Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPresentation();
builder.Services.AddApplicationLayer(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructureLayer(builder.Configuration);

var origins = new List<string>();

if (builder.Environment.IsDevelopment())
{
    origins.Add("http://localhost:4200");
}
else
{
    origins.Add("https://mango-coast-09be74403.2.azurestaticapps.net");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(origins.ToArray())
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandlerMw();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await SeedData();

app.Run();

async Task SeedData()
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var songsElasticService = scope.ServiceProvider.GetRequiredService<SongsElasticService>();
            var playlistsElasticService = scope.ServiceProvider.GetRequiredService<PlaylistsElasticService>();
            var usersElasticService = scope.ServiceProvider.GetRequiredService<UsersElasticService>();
            var genresElasticService = scope.ServiceProvider.GetRequiredService<GenresElasticService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();

            List<Task> createIndexTasks = new()
            {
                songsElasticService.CreateIndexIfNotExistsAsync(ElasticIndex.SongsIndex),
                playlistsElasticService.CreateIndexIfNotExistsAsync(ElasticIndex.PlaylistsIndex),
                usersElasticService.CreateIndexIfNotExistsAsync(ElasticIndex.UsersIndex),
                genresElasticService.CreateIndexIfNotExistsAsync(ElasticIndex.GenresIndex)
            };

            await Task.WhenAll(createIndexTasks);

            List<Task> reindexTasks = new()
            {
                songsElasticService.ReindexAllAsync(),
                playlistsElasticService.ReindexAllAsync(),
                usersElasticService.ReindexAllAsync(),
                genresElasticService.ReindexAllAsync()
            };

            await Task.WhenAll(reindexTasks);
            await SeedRoles.Seed(roleManager);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occured during seeding data");
        }
    }
}
