using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PhotoTypeConfig : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder
            .HasQueryFilter(b => !b.isDeleted);

        builder
            .HasIndex(b => b.isDeleted)
            .HasFilter("IsDeleted = 0");

        builder
            .HasKey(b => b.PhotoId);

        builder
            .Property(b => b.URL)
            .IsRequired();

        builder
            .Property(b => b.PublicId)
            .HasMaxLength(100)
            .IsRequired();
    }
}
