using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PhotoTypeConfig : IEntityTypeConfiguration<Photo>
{
    public virtual void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder
            .HasKey(b => b.PhotoId);

        builder
            .HasQueryFilter(b => !b.isDeleted);

        builder
            .HasIndex(b => b.isDeleted)
            .HasFilter("[IsDeleted] = 0");

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
