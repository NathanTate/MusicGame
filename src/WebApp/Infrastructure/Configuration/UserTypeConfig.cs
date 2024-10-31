using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Infrastructure.Configuration;
internal class UserTypeConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .Property(b => b.Id)
            .HasMaxLength(128);

        builder
            .HasIndex(b => b.Email)
            .IsUnique();

        builder
            .Property(b => b.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder
            .Property(b => b.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(b => b.RefreshToken)
            .HasMaxLength(64)
            .IsRequired(false);

        builder
            .Property(b => b.RefreshTokenExpiryTime)
            .IsRequired(false);

        builder
            .Ignore(b => b.Roles);
    }
}
