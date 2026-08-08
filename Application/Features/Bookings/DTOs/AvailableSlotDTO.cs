namespace Application.Features.Bookings.DTOs
{
    public class AvailableSlotDTO
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}