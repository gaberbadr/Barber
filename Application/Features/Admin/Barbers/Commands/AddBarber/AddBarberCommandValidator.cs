using FluentValidation;

namespace Application.Features.Admin.Barbers.Commands.AddBarber
{
    public class AddBarberCommandValidator : AbstractValidator<AddBarberCommand>
    {
        public AddBarberCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.BookingDurationMinutes)
                .GreaterThan(0).WithMessage("Booking duration must be greater than 0.");
        }
    }
}