using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Barbers.Queries.GetMyBookings
{
    public class GetMyBarberBookingsQuery : IRequest<ErrorOr<List<BookingDTO>>>
    {
        public string BarberId { get; set; } = string.Empty;
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}