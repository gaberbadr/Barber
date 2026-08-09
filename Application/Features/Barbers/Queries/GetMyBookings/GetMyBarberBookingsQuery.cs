using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;
using Application.Common.Pagination;

namespace Application.Features.Barbers.Queries.GetMyBookings
{
    public class GetMyBarberBookingsQuery : PaginationRequest, IRequest<ErrorOr<PaginationResponse<BookingDTO>>>
    {
        public string BarberId { get; set; } = string.Empty;
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }
}