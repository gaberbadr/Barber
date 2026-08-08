using FluentValidation;

namespace Application.Features.Barbers.Commands.UpdateBookingSettings
{
    public class UpdateBookingSettingsCommandValidator : AbstractValidator<UpdateBookingSettingsCommand>
    {
        private static readonly int[] AllowedDurations = { 15, 30, 45, 60 };

        public UpdateBookingSettingsCommandValidator()
        {
            RuleFor(x => x.BookingDurationMinutes)
                .Must(d => AllowedDurations.Contains(d))
                .WithMessage("Booking duration must be 15, 30, 45, or 60 minutes.");
        }
    }
}