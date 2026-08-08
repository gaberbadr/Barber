using Domain.Entities;

namespace Domain.Entities
{
    public class BarberWorkingHour : BaseEntity<int>
    {
        public string BarberId { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }

        // Navigation
        public ApplicationUser Barber { get; set; } = null!;
    }
}