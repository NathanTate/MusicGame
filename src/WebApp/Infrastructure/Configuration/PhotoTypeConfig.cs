using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class PhotoTypeConfig : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
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
