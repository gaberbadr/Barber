using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
    {
        public void Configure(EntityTypeBuilder<BookingItem> builder)
        {
            builder.HasKey(bi => bi.Id);

            builder.Property(bi => bi.ServiceNameSnapshot)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(bi => bi.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(bi => bi.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(bi => bi.Quantity)
                .IsRequired();

            builder.HasOne(bi => bi.Booking)
                .WithMany(b => b.BookingItems)
                .HasForeignKey(bi => bi.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bi => bi.Service)
                .WithMany(s => s.BookingItems)
                .HasForeignKey(bi => bi.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}