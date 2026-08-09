using System;
using System.Collections.Generic;

namespace Requests.Bookings
{
    public class CreateBookingRequest
    {
        public string BarberId { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public string? CouponCode { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
