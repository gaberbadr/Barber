using Domain.Entities;

namespace Domain.Entities
{
    public class Coupon : BaseEntity<int>
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int? UsageLimit { get; set; }
        public int TimesUsed { get; set; }

        // Navigation
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}