using FluentValidation;

namespace Application.Features.Barbers.Commands.UpdateWorkingHours
{
    public class UpdateWorkingHoursCommandValidator : AbstractValidator<UpdateWorkingHoursCommand>
    {
        public UpdateWorkingHoursCommandValidator()
        {
            RuleFor(x => x.WorkingHours)
                .NotEmpty().WithMessage("At least one working hour entry is required.");

            RuleForEach(x => x.WorkingHours).ChildRules(wh =>
            {
                wh.RuleFor(w => w.OpeningTime)
                    .LessThan(w => w.ClosingTime)
                    .When(w => !w.IsClosed)
                    .WithMessage("Opening time must be before closing time.");
            });
        }
    }
}