using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class SongTypeConfig : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> builder)
    {
        builder
            .HasQueryFilter(b => !b.isDeleted);

        builder
            .HasIndex(b => b.isDeleted)
            .HasFilter("IsDeleted = 0");

        builder
            .HasKey(b => b.SongId);

        builder
            .HasIndex(b => b.Name)
            .IsUnique();

        builder
            .Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(b => b.Path)
            .IsRequired();

        builder.
            Property(b => b.Duration)
            .IsRequired();

        builder
            .Property(b => b.PhotoId)
            .IsRequired(false);

        builder
            .HasOne(b => b.Photo)
            .WithMany(b => b.Songs)
            .HasForeignKey(b => b.PhotoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(b => b.UserId)
            .IsRequired(true);

        builder
            .HasOne(b => b.User)
            .WithMany(b => b.Songs)
            .HasForeignKey(b => b.UserId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        builder
           .HasMany(b => b.Genres)
           .WithMany(b => b.Songs)
           .UsingEntity<Dictionary<string, object>>(
               "SongGenre",
               j => j
                   .HasOne<Genre>()
                   .WithMany()
                   .HasForeignKey("GenreId"),
               j => j
                   .HasOne<Song>()
                   .WithMany()
                   .HasForeignKey("SongId")
           );

        builder
           .HasMany(b => b.UserLikes)
           .WithMany(b => b.LikedSongs)
           .UsingEntity<Dictionary<string, object>>(
               "SongLike",
               j => j
                   .HasOne<User>()
                   .WithMany()
                   .HasForeignKey("UserId")
                   .OnDelete(DeleteBehavior.NoAction),
               j => j
                   .HasOne<Song>()
                   .WithMany()
                   .HasForeignKey("SongId")
           );
    }
}
