using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Bookings.Queries.GetMyHistory
{
    public class GetMyBookingHistoryQuery : IRequest<ErrorOr<List<BookingDTO>>>
    {
        public string CustomerId { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}