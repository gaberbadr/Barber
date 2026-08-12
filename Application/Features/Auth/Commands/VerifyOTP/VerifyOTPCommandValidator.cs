using FluentValidation;

namespace Application.Features.Auth.Commands.VerifyOTP
{
    public class VerifyOTPCommandValidator : AbstractValidator<VerifyOTPCommand>
    {
        public VerifyOTPCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("كود التحقق مطلوب.")
                .Length(6).WithMessage("كود التحقق لازم يكون 6 أرقام.");
        }
    }
}