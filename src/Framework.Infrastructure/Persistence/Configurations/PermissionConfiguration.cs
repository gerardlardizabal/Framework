using Framework.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Framework.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("PermissionDefinitions");
        builder.Property(permission => permission.Group).HasMaxLength(64).IsRequired();
        builder.Property(permission => permission.Name).HasMaxLength(128).IsRequired();
        builder.Property(permission => permission.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(512);
        builder.HasIndex(permission => permission.Name).IsUnique();
    }
}
