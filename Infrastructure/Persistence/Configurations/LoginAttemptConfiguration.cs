using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.HasKey(la => la.Id);

            builder.Property(la => la.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(la => la.AttemptedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(la => la.IpAddress)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(la => la.IsSuccessful)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(la => la.Email);

            builder.HasIndex(la => new { la.Email, la.AttemptedAt });

            builder.ToTable("LoginAttempts");
        }
    }
}