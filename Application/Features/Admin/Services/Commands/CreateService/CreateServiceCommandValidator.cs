using FluentValidation;

namespace Application.Features.Admin.Services.Commands.CreateService
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceCommandValidator()
        {
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
