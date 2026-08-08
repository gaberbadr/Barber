using MediatR;
using ErrorOr;
using Application.Features.Barbers.DTOs;

namespace Application.Features.Barbers.Commands.UpdateBookingSettings
{
    public class UpdateBookingSettingsCommand : IRequest<ErrorOr<BarberDTO>>
    {
        public string BarberId { get; set; } = string.Empty;
        public int BookingDurationMinutes { get; set; }
        public bool AcceptingBookings { get; set; }
    }
}