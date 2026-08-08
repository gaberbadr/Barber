using Domain.Entities;
using Domain.Enums;

namespace Domain.Entities
{
    public class Booking : BaseEntity<int>
    {
        public string CustomerId { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public int? CouponId { get; set; }
        public string? CouponCodeSnapshot { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public DateTime? CancelledAt { get; set; }
        public string? CancelledBy { get; set; }

        // Navigation
        public ApplicationUser Customer { get; set; } = null!;
        public ApplicationUser Barber { get; set; } = null!;
        public Coupon? Coupon { get; set; }
        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}