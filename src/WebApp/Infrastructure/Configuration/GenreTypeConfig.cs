using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;
internal class GenreTypeConfig : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder
            .HasQueryFilter(b => !b.isDeleted);

        builder
            .HasIndex(b => b.isDeleted)
            .HasFilter("IsDeleted = 0");

        builder
            .HasKey(b => b.GenreId);

        builder
            .Property(b => b.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(b => b.NormalizedName)
            .HasMaxLength(100)
            .HasComputedColumnSql("Upper([Name])", stored: true);

        builder
            .Property(b => b.IsSystemDefined)
            .HasDefaultValue(false)
            .IsRequired();
    }
}
