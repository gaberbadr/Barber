using Domain.Entities;

namespace Domain.Entities
{
    public class ShopWorkingHour : BaseEntity<int>
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }
}