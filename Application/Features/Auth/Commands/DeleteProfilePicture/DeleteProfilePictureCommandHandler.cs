using Application.Interfaces;
using Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Auth.Commands.DeleteProfilePicture
{
    public class DeleteProfilePictureCommandHandler : IRequestHandler<DeleteProfilePictureCommand, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;

        public DeleteProfilePictureCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ErrorOr<Success>> Handle(
            DeleteProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            // Get the user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Error.NotFound("auth.user.not.found", "المستخدم ده مش موجود.");
            }

            // Check if user has a profile picture
            if (string.IsNullOrEmpty(user.ProfilePicturePublicId))
            {
                return Error.NotFound("image.not.found", "المستخدم معندوش صورة شخصية.");
            }

            try
            {
                // Delete from Cloudinary
                var deleteResult = await _cloudinaryService.DeleteImageAsync(user.ProfilePicturePublicId);
                if (deleteResult.IsError)
                {
                    return deleteResult.Errors;
                }

                // Clear the URL and Public ID from user
                user.ProfilePictureUrl = null;
                user.ProfilePicturePublicId = null;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return Error.Failure("image.delete.failed", "فشل مسح الصورة الشخصية.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure("image.delete.exception", $"حصل مشكلة: {ex.Message}");
            }
        }


    }
}