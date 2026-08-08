using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.DiscountPercentage)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(c => c.StartDate)
                .IsRequired();

            builder.Property(c => c.ExpiryDate)
                .IsRequired();

            builder.HasIndex(c => c.Code)
                .IsUnique();
        }
    }
}