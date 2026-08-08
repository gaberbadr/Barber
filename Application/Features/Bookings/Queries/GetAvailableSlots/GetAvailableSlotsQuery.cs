using MediatR;
using ErrorOr;
using Application.Features.Bookings.DTOs;

namespace Application.Features.Bookings.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQuery : IRequest<ErrorOr<List<AvailableSlotDTO>>>
    {
        public string BarberId { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
    }
}