using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Bookings.Queries.GetMyUpcoming
{
    public class GetMyUpcomingBookingsQuery : IRequest<ErrorOr<List<BookingDTO>>>
    {
        public string CustomerId { get; set; } = string.Empty;
    }
}