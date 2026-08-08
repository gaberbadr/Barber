using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BarberWorkingHourConfiguration : IEntityTypeConfiguration<BarberWorkingHour>
    {
        public void Configure(EntityTypeBuilder<BarberWorkingHour> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BarberId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(b => b.DayOfWeek)
                .IsRequired();

            builder.HasOne(b => b.Barber)
                .WithMany(u => u.BarberWorkingHours)
                .HasForeignKey(b => b.BarberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => new { b.BarberId, b.DayOfWeek })
                .IsUnique();
        }
    }
}