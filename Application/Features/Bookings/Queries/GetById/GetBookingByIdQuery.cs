using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Bookings.Queries.GetById
{
    public class GetBookingByIdQuery : IRequest<ErrorOr<BookingDTO>>
    {
        public int BookingId { get; set; }
        public string RequestingUserId { get; set; } = string.Empty;
    }
}