using Domain.Entities;
using Domain.Primitives;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public sealed class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Photo> Photos { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var softDeleteEntires = ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entityEntry in softDeleteEntires)
        {
            entityEntry.State = EntityState.Modified;
            entityEntry.Property(nameof(ISoftDeletable.isDeleted)).CurrentValue = true;
            entityEntry.Property(nameof(ISoftDeletable.DeletedOnUtc)).CurrentValue = DateTime.UtcNow;
        }

        var modifiedSoftDeletedEntries = ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Modified
                    && e.Entity.isDeleted
                    && !e.Property(nameof(ISoftDeletable.isDeleted)).IsModified);

        if (modifiedSoftDeletedEntries.Any())
        {
            throw new InvalidOperationException("Deleted entities can't be modified");
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
