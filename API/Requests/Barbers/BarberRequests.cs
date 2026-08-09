using System;
using System.Collections.Generic;

namespace Requests.Barbers
{
    public class UpdateBookingSettingsRequest
    {
        public int BookingDurationMinutes { get; set; }
        public bool AcceptingBookings { get; set; }
    }

    public class UpdateWorkingHoursRequest
    {
        public List<WorkingHourItem> WorkingHours { get; set; } = new();
    }

    public class WorkingHourItem
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
