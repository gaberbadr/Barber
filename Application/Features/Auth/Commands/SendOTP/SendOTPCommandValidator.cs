using FluentValidation;

namespace Application.Features.Auth.Commands.SendOTP
{
    public class SendOTPCommandValidator : AbstractValidator<SendOTPCommand>
    {
        public SendOTPCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");
        }
    }
}