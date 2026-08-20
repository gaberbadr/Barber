using Application.Features.Bookings.DTOs;
using Domain.Enums;
using ErrorOr;
using MediatR;

namespace Application.Features.Bookings.Commands.UpdateStatus
{
    public class UpdateBookingStatusCommand : IRequest<ErrorOr<BookingDTO>>
    {
        public int BookingId { get; set; }
        public BookingStatus NewStatus { get; set; }
        public string RequestingUserId { get; set; } = string.Empty;
    }
}
