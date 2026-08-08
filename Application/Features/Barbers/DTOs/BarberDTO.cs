namespace Application.Features.Barbers.DTOs
{
    public class BarberDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int BookingDurationMinutes { get; set; }
        public bool AcceptingBookings { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BarberWorkingHourDTO> WorkingHours { get; set; } = new();
    }

    public class BarberWorkingHourDTO
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }
}