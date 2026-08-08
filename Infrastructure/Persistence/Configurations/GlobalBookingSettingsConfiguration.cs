using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class GlobalBookingSettingsConfiguration : IEntityTypeConfiguration<GlobalBookingSettings>
    {
        public void Configure(EntityTypeBuilder<GlobalBookingSettings> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.MaximumBookingAdvanceDays)
                .IsRequired();

            builder.Property(g => g.CancellationWindowHours)
                .IsRequired();
        }
    }
}