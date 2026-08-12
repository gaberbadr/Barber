using FluentValidation;

namespace Application.Features.Admin.Coupons.Commands.CreateCoupon
{
    public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
    {
        public CreateCouponCommandValidator(TimeProvider timeProvider)
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("كود الخصم مطلوب.")
                .Length(1, 50).WithMessage("كود الخصم لازم يكون بين حرف و 50 حرف.")
                .Matches(@"^[A-Z0-9]+$").WithMessage("كود الخصم لازم يكون حروف كبيرة وأرقام بس.");

            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0).WithMessage("نسبة الخصم لازم تكون أكتر من صفر.")
                .LessThanOrEqualTo(100).WithMessage("نسبة الخصم مينفعش تعدي 100.");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.ExpiryDate).WithMessage("تاريخ البداية لازم يكون قبل تاريخ الانتهاء.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(timeProvider.GetLocalNow().DateTime).WithMessage("تاريخ الانتهاء لازم يكون في المستقبل.");

            RuleFor(x => x.UsageLimit)
                .GreaterThan(0).WithMessage("حد الاستخدام لازم يكون أكتر من صفر.")
                .When(x => x.UsageLimit.HasValue);
        }
    }
}