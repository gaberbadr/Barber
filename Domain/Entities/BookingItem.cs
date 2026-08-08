using Domain.Entities;

namespace Domain.Entities
{
    public class BookingItem : BaseEntity<int>
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceNameSnapshot { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal TotalPrice { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}