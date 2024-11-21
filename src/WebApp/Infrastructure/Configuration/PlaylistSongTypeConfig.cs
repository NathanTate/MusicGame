using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PlaylistSongTypeConfig : IEntityTypeConfiguration<PlaylistSong>
{
    public void Configure(EntityTypeBuilder<PlaylistSong> builder)
    {
        builder
            .HasQueryFilter(b => !b.Playlist.isDeleted && !b.Song.isDeleted);

        builder
            .HasKey(x => new { x.PlaylistId, x.SongId });

        builder
            .HasOne(b => b.Playlist)
            .WithMany(b => b.Songs)
            .HasForeignKey(b => b.PlaylistId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(b => b.Song)
            .WithMany(b => b.Playlists)
            .HasForeignKey(b => b.SongId);

        builder
            .Property(b => b.Position)
            .HasDefaultValue(1);
    }
}
