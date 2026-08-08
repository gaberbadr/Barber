using Domain.Entities;

namespace Domain.Entities
{
    public class GlobalBookingSettings : BaseEntity<int>
    {
        public int MaximumBookingAdvanceDays { get; set; } = 7;
        public int CancellationWindowHours { get; set; } = 16;
    }
}