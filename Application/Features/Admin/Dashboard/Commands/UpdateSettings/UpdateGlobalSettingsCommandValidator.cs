using FluentValidation;

namespace Application.Features.Admin.Dashboard.Commands.UpdateSettings
{
    public class UpdateGlobalSettingsCommandValidator : AbstractValidator<UpdateGlobalSettingsCommand>
    {
        public UpdateGlobalSettingsCommandValidator()
        {
            RuleFor(x => x.MaximumBookingAdvanceDays)
                .GreaterThan(0).WithMessage("أقصى عدد أيام للحجز لازم يكون أكتر من صفر.")
                .LessThanOrEqualTo(365).WithMessage("أقصى عدد أيام للحجز مينفعش يعدي 365 يوم.");

            RuleFor(x => x.CancellationWindowHours)
                .GreaterThanOrEqualTo(0).WithMessage("ساعات فترة الإلغاء لازم تكون صفر أو أكتر.")
                .LessThanOrEqualTo(168).WithMessage("ساعات فترة الإلغاء مينفعش تعدي 168 ساعة (7 أيام).");
        }
    }
}