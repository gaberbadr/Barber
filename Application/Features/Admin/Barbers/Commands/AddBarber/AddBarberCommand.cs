using MediatR;
using ErrorOr;
using Application.Features.Barbers.DTOs;

namespace Application.Features.Admin.Barbers.Commands.AddBarber
{
    public class AddBarberCommand : IRequest<ErrorOr<BarberDTO>>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = string.Empty;
        public int BookingDurationMinutes { get; set; } = 30;
        public bool AcceptingBookings { get; set; } = true;
    }
}