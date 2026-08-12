using FluentValidation;

namespace Application.Features.Admin.Barbers.Commands.AddBarber
{
    public class AddBarberCommandValidator : AbstractValidator<AddBarberCommand>
    {
        public AddBarberCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("الاسم بالكامل مطلوب.")
                .Length(2, 100).WithMessage("الاسم بالكامل لازم يكون بين 2 و 100 حرف.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح.");

            RuleFor(x => x.BookingDurationMinutes)
                .GreaterThan(0).WithMessage("مدة الحجز لازم تكون أكتر من صفر.");
        }
    }
}