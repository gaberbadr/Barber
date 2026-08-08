using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Zero.Infrastructure.Persistence.Configurations
{
    /// Entity Framework Core configuration for RefreshTokenTable entity.
    /// Defines table name, column properties, indexes, and relationships.
    public class RefreshTokenTableConfiguration : IEntityTypeConfiguration<RefreshTokenTable>
    {
        public void Configure(EntityTypeBuilder<RefreshTokenTable> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rt => rt.UserId)
                .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.CreatedByIp)
                .HasMaxLength(50);

            builder.Property(rt => rt.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // Relationships
            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.Token).IsUnique();

            // Table name
            builder.ToTable("RefreshTokens");
        }
    }
}