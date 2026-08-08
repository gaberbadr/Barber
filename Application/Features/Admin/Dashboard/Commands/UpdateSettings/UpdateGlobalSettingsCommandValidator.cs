using FluentValidation;

namespace Application.Features.Admin.Dashboard.Commands.UpdateSettings
{
    public class UpdateGlobalSettingsCommandValidator : AbstractValidator<UpdateGlobalSettingsCommand>
    {
        public UpdateGlobalSettingsCommandValidator()
        {
            RuleFor(x => x.MaximumBookingAdvanceDays)
                .GreaterThan(0).WithMessage("Maximum booking advance days must be greater than 0.")
                .LessThanOrEqualTo(365).WithMessage("Maximum booking advance days cannot exceed 365.");

            RuleFor(x => x.CancellationWindowHours)
                .GreaterThanOrEqualTo(0).WithMessage("Cancellation window hours must be 0 or greater.")
                .LessThanOrEqualTo(168).WithMessage("Cancellation window hours cannot exceed 168 (7 days).");
        }
    }
}