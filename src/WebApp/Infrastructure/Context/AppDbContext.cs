using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

internal sealed class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Song> Songs { get; set; }
}
