using FluentValidation;

namespace Application.Features.Bookings.Commands.Create
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.BarberId)
                .NotEmpty().WithMessage("Barber is required.");

            RuleFor(x => x.BookingDate)
                .NotEmpty().WithMessage("Booking date is required.")
                .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Booking date cannot be in the past.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required.");

            RuleFor(x => x.ServiceIds)
                .NotEmpty().WithMessage("At least one service must be selected.");
        }
    }
}