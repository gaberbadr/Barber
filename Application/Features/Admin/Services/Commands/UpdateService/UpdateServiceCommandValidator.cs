using FluentValidation;

namespace Application.Features.Admin.Services.Commands.UpdateService
{
    public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
    {
        public UpdateServiceCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("رقم الخدمة مطلوب.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("الاسم مطلوب.")
                .MaximumLength(100).WithMessage("الاسم مينفعش يعدي 100 حرف.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("الوصف مينفعش يعدي 500 حرف.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("السعر لازم يكون صفر أو أكتر.");
        }
    }
}
