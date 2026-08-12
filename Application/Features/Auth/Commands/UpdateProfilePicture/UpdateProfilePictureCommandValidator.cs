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
                .NotEmpty().WithMessage("رقم المستخدم مطلوب.");

            RuleFor(x => x.ProfilePictureFile)
                .NotNull().WithMessage("ملف الصورة الشخصية مطلوب.");

            When(x => x.ProfilePictureFile != null, () =>
            {
                RuleFor(x => x.ProfilePictureFile!.Length)
                    .LessThanOrEqualTo(MaxFileSizeBytes)
                    .WithMessage($"حجم الملف مينفعش يعدي 5 ميجا.");

                RuleFor(x => x.ProfilePictureFile!.FileName)
                    .Must(fileName => AllowedExtensions.Any(ext => 
                        fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("الملف لازم يكون بصيغة صورة صحيحة (.jpg, .jpeg, .png, .gif, .webp).");

                RuleFor(x => x.ProfilePictureFile!.ContentType)
                    .Must(contentType => contentType != null && 
                        (contentType.StartsWith("image/") || 
                         contentType == "application/octet-stream"))
                    .WithMessage("الملف لازم يكون صورة صحيحة.");
            });
        }
    }
}