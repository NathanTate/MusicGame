using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PlaylistLikeTypeConfig : IEntityTypeConfiguration<PlaylistLike>
{
    public void Configure(EntityTypeBuilder<PlaylistLike> builder)
    {
        builder
            .HasQueryFilter(b => !b.Playlist.isDeleted && !b.User.isDeleted);

        builder
            .HasKey(x => new { x.PlaylistId, x.UserId });

        builder
            .HasOne(b => b.Playlist)
            .WithMany(b => b.UserLikes)
            .HasForeignKey(b => b.PlaylistId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(b => b.User)
            .WithMany(b => b.LikedPlaylists)
            .HasForeignKey(b => b.UserId);
    }
}
