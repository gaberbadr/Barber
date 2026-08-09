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

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits.");
        }
    }
}