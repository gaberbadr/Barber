using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Bookings.Commands.Cancel
{
    public class CancelBookingCommand : IRequest<ErrorOr<BookingDTO>>
    {
        public int BookingId { get; set; }
        public string CancelledByUserId { get; set; } = string.Empty;
    }
}