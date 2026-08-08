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
                return Error.NotFound("auth.user.not.found", "User not found.");
            }

            // Check if user has a profile picture
            if (string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                return Error.NotFound("image.not.found", "User does not have a profile picture.");
            }

            try
            {
                // Extract public ID and delete from Cloudinary
                var publicId = ExtractPublicIdFromUrl(user.ProfilePictureUrl);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }

                // Clear the URL from user
                user.ProfilePictureUrl = null;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return Error.Failure("image.delete.failed", "Failed to remove profile picture.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure("image.delete.exception", $"An error occurred: {ex.Message}");
            }
        }

        private string? ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.Segments;

                var uploadIndex = Array.FindIndex(segments, s => s.Contains("upload", StringComparison.OrdinalIgnoreCase));
                if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
                {
                    var lastSegment = segments[uploadIndex + 1].TrimEnd('/');
                    if (lastSegment.StartsWith("v"))
                    {
                        lastSegment = lastSegment.Substring(lastSegment.IndexOf('/') + 1);
                    }
                    var publicId = Path.GetFileNameWithoutExtension(lastSegment);
                    return publicId;
                }
            }
            catch
            {
                // If extraction fails, return null
            }

            return null;
        }
    }
}