using FluentValidation;

namespace Application.Features.Auth.Commands.SendOTP
{
    public class SendOTPCommandValidator : AbstractValidator<SendOTPCommand>
    {
        public SendOTPCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");
        }
    }
}