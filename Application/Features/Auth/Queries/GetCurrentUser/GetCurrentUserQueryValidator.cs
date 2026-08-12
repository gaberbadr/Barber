using FluentValidation;

namespace Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("رقم المستخدم مطلوب.");
        }
    }
}