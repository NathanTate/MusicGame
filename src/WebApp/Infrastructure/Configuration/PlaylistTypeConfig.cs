using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PlaylistTypeConfig : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder
            .HasQueryFilter(b => !b.isDeleted);

        builder
            .HasIndex(b => b.isDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder
            .HasKey(b => b.PlaylistId);

        builder
            .Property(b => b.Name)
            .HasMaxLength(100)
            .IsRequired(true);

        builder
            .Property(b => b.IsPrivate)
            .HasDefaultValue(false)
            .IsRequired(true);

        builder
            .Property(b => b.TotalDuration)
            .HasDefaultValue(0)
            .IsRequired(true);

        builder
            .Property(b => b.SongsCount)
            .HasDefaultValue(0)
            .IsRequired(true);

        builder
            .Property(b => b.UserId)
            .IsRequired(true);

        builder
            .HasOne(b => b.Photo)
            .WithMany(b => b.Playlists)
            .HasForeignKey(b => b.PhotoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(b => b.User)
            .WithMany(b => b.Playlists)
            .HasForeignKey(b => b.UserId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        builder
           .HasMany(b => b.Songs)
           .WithMany(b => b.Playlists)
           .UsingEntity<Dictionary<string, object>>(
               "PlaylistSong",
               j => j
                   .HasOne<Song>()
                   .WithMany()
                   .HasForeignKey("SongId")
                   .OnDelete(DeleteBehavior.NoAction),
               j => j
                   .HasOne<Playlist>()
                   .WithMany()
                   .HasForeignKey("PlaylistId")
           );

        builder
            .HasMany(b => b.UserLikes)
            .WithMany(b => b.LikedPlaylists)
            .UsingEntity<Dictionary<string, object>>(
                "PlaylistLike",
                j => j
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.NoAction),
                j => j
                    .HasOne<Playlist>()
                    .WithMany()
                    .HasForeignKey("PlaylistId")
                    
            );

    }
}
