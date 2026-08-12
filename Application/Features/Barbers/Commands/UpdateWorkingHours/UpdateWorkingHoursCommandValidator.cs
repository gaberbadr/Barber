using FluentValidation;

namespace Application.Features.Barbers.Commands.UpdateWorkingHours
{
    public class UpdateWorkingHoursCommandValidator : AbstractValidator<UpdateWorkingHoursCommand>
    {
        public UpdateWorkingHoursCommandValidator()
        {
            RuleFor(x => x.WorkingHours)
                .NotEmpty().WithMessage("لازم تدخل موعد عمل واحد على الأقل.");

            RuleForEach(x => x.WorkingHours).ChildRules(wh =>
            {
                wh.RuleFor(w => w.OpeningTime)
                    .LessThan(w => w.ClosingTime)
                    .When(w => !w.IsClosed)
                    .WithMessage("وقت الفتح لازم يكون قبل وقت القفل.");
            });
        }
    }
}