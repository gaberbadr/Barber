using FluentValidation;

namespace Application.Features.Auth.Commands.RevokeToken
{
    public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
    {
        public RevokeTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("توكن التجديد مطلوب.");
        }
    }
}