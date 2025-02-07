using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class SongLikeTypeConfig : IEntityTypeConfiguration<SongLike>
{
    public void Configure(EntityTypeBuilder<SongLike> builder)
    {
        builder
            .HasQueryFilter(b => !b.Song.isDeleted && !b.User.isDeleted);

        builder
            .HasKey(x => new { x.SongId, x.UserId });

        builder
            .HasOne(b => b.Song)
            .WithMany(b => b.UserLikes)
            .HasForeignKey(b => b.SongId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(b => b.User)
            .WithMany(b => b.LikedSongs)
            .HasForeignKey(b => b.UserId);
    }
}
