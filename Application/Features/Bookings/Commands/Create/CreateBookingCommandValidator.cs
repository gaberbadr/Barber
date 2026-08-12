using FluentValidation;

namespace Application.Features.Bookings.Commands.Create
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator(TimeProvider timeProvider)
        {
            RuleFor(x => x.BarberId)
                .NotEmpty().WithMessage("الحلاق مطلوب.");

            RuleFor(x => x.BookingDate)
                .NotEmpty().WithMessage("تاريخ الحجز مطلوب.")
                .Must(date => date >= DateOnly.FromDateTime(timeProvider.GetLocalNow().Date))
                .WithMessage("تاريخ الحجز مينفعش يكون في الماضي.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("وقت البداية مطلوب.");

            RuleFor(x => x.ServiceIds)
                .NotEmpty().WithMessage("لازم تختار خدمة واحدة على الأقل.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("الاسم بالكامل مطلوب.")
                .Length(2, 100).WithMessage("الاسم لازم يكون بين 2 و 100 حرف.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("رقم الموبايل مطلوب.")
                .Matches(@"^\d{10,15}$").WithMessage("رقم الموبايل لازم يكون بين 10 و 15 رقم.");
        }
    }
}