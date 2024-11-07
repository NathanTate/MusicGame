using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PhotoTypeConfig<T> : IEntityTypeConfiguration<T> where T: Photo
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder
            .HasQueryFilter(b => !b.isDeleted);

        builder
            .HasIndex(b => b.isDeleted)
            .HasFilter("IsDeleted = 0");

        builder
            .Property(b => b.URL)
            .IsRequired();

        builder
            .Property(b => b.Size)
            .IsRequired();

        builder
           .Property(b => b.ContentType)
           .HasMaxLength(30)
           .IsRequired();
    }
}

internal class UserPhotoTypeConfig : PhotoTypeConfig<UserPhoto>
{
    public override void Configure(EntityTypeBuilder<UserPhoto> builder)
    {
        builder
            .HasKey(b => b.UserPhotoId);

        base.Configure(builder);
    }
}

internal class MediaPhotoTypeConfig : PhotoTypeConfig<MediaPhoto>
{
    public override void Configure(EntityTypeBuilder<MediaPhoto> builder)
    {
        builder
            .HasKey(b => b.MediaPhotoId);

        base.Configure(builder);
    }
}
