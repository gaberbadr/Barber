namespace Application.Features.Admin.Dashboard.DTOs
{
    public class DashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BlockedUsers { get; set; }
        public int TotalBarbers { get; set; }
        public int ActiveBarbers { get; set; }
        public int TotalServices { get; set; }
        public int TotalConfirmedBookings { get; set; }
        public int TotalCancelledBookings { get; set; }
        public int TodayConfirmedBookings { get; set; }
        public int ThisMonthConfirmedBookings { get; set; }
        public decimal TotalConfirmedRevenue { get; set; }
        public decimal ThisMonthConfirmedRevenue { get; set; }
    }

    public class MonthlyReportDTO
    {
        public string Month { get; set; } = string.Empty;
        public int ConfirmedBookingCount { get; set; }
        public int CancelledBookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopBarberDTO
    {
        public string BarberId { get; set; } = string.Empty;
        public string BarberName { get; set; } = string.Empty;
        public int ConfirmedBookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopServiceDTO
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class GlobalSettingsDTO
    {
        public int Id { get; set; }
        public int MaximumBookingAdvanceDays { get; set; }
        public int CancellationWindowHours { get; set; }
    }

    public class ShopWorkingHourDTO
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }

    public class AdminBookingDTO
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string BarberName { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}