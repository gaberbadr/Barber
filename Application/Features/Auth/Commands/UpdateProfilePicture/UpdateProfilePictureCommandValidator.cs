using FluentValidation;

namespace Application.Features.Auth.Commands.UpdateProfilePicture
{
    public class UpdateProfilePictureCommandValidator : AbstractValidator<UpdateProfilePictureCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UpdateProfilePictureCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.ProfilePictureFile)
                .NotNull().WithMessage("Profile picture file is required.");

            When(x => x.ProfilePictureFile != null, () =>
            {
                RuleFor(x => x.ProfilePictureFile!.Length)
                    .LessThanOrEqualTo(MaxFileSizeBytes)
                    .WithMessage($"File size must not exceed 5 MB.");

                RuleFor(x => x.ProfilePictureFile!.FileName)
                    .Must(fileName => AllowedExtensions.Any(ext => 
                        fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("File must be a valid image format (.jpg, .jpeg, .png, .gif, .webp).");

                RuleFor(x => x.ProfilePictureFile!.ContentType)
                    .Must(contentType => contentType != null && 
                        (contentType.StartsWith("image/") || 
                         contentType == "application/octet-stream"))
                    .WithMessage("File must be a valid image.");
            });
        }
    }
}