using FluentValidation;

namespace Application.Features.Admin.Coupons.Commands.CreateCoupon
{
    public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
    {
        public CreateCouponCommandValidator(TimeProvider timeProvider)
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Coupon code is required.")
                .Length(1, 50).WithMessage("Coupon code must be between 1 and 50 characters.")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Coupon code must contain only uppercase letters and numbers.");

            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0).WithMessage("Discount percentage must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Discount percentage cannot exceed 100.");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.ExpiryDate).WithMessage("Start date must be before expiry date.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(timeProvider.GetLocalNow().DateTime).WithMessage("Expiry date must be in the future.");

            RuleFor(x => x.UsageLimit)
                .GreaterThan(0).WithMessage("Usage limit must be greater than 0.")
                .When(x => x.UsageLimit.HasValue);
        }
    }
}