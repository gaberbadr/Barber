using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetAllBookings
{
    public class GetAllBookingsQuery : IRequest<ErrorOr<List<AdminBookingDTO>>>
    {
        public DateOnly? Date { get; set; }
        public string? BarberId { get; set; }
        public string? CustomerId { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}