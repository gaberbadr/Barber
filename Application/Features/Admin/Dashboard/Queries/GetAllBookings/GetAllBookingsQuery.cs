using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;
using Application.Common.Pagination;

namespace Application.Features.Admin.Dashboard.Queries.GetAllBookings
{
    public class GetAllBookingsQuery : PaginationRequest, IRequest<ErrorOr<PaginationResponse<AdminBookingDTO>>>
    {
        public DateOnly? Date { get; set; }
        public string? BarberId { get; set; }
        public string? CustomerId { get; set; }
        public string? Status { get; set; }
    }
}