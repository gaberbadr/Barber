using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.CustomerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(b => b.BarberId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(b => b.SubTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(b => b.Discount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(b => b.CouponCodeSnapshot)
                .HasMaxLength(50);

            builder.Property(b => b.CustomerNameSnapshot)
                .HasMaxLength(255);

            builder.Property(b => b.CustomerPhoneSnapshot)
                .HasMaxLength(20);

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(b => b.CancelledBy)
                .HasMaxLength(450);

            // Relationships
            builder.HasOne(b => b.Customer)
                .WithMany(u => u.CustomerBookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Barber)
                .WithMany(u => u.BarberBookings)
                .HasForeignKey(b => b.BarberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Coupon)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(b => b.BarberId);
            builder.HasIndex(b => b.CustomerId);
            builder.HasIndex(b => b.BookingDate);
            builder.HasIndex(b => b.StartTime);
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => new { b.BarberId, b.BookingDate, b.StartTime });
            builder.HasIndex(b => new { b.CustomerId, b.BookingDate, b.Status });
        }
    }
}