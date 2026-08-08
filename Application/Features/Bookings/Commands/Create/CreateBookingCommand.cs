using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Bookings.Commands.Create
{
    public class CreateBookingCommand : IRequest<ErrorOr<BookingDTO>>
    {
        public string CustomerId { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public string? CouponCode { get; set; }
    }
}