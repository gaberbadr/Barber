using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;
using Application.Common.Pagination;

namespace Application.Features.Bookings.Queries.GetMyHistory
{
    public class GetMyBookingHistoryQuery : PaginationRequest, IRequest<ErrorOr<PaginationResponse<BookingDTO>>>
    {
        public string CustomerId { get; set; } = string.Empty;
    }
}