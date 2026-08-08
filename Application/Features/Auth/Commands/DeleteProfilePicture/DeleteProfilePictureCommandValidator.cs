using FluentValidation;

namespace Application.Features.Auth.Commands.DeleteProfilePicture
{
    public class DeleteProfilePictureCommandValidator : AbstractValidator<DeleteProfilePictureCommand>
    {
        public DeleteProfilePictureCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}