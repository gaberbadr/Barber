using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ShopWorkingHourConfiguration : IEntityTypeConfiguration<ShopWorkingHour>
    {
        public void Configure(EntityTypeBuilder<ShopWorkingHour> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.DayOfWeek)
                .IsRequired();

            builder.HasIndex(s => s.DayOfWeek)
                .IsUnique();
        }
    }
}