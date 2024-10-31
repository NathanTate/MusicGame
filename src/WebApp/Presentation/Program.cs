using Application;
using Infrastructure;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);

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

app.UseCors();

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

            await SeedRoles.Seed(roleManager);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occured during seeding data");
        }
    }
}
