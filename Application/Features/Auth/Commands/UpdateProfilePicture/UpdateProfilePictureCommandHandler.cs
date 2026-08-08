using Application.Features.Auth.DTOs;
using AutoMapper;
using Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.Interfaces;
using Error = ErrorOr.Error;

namespace Application.Features.Auth.Commands.UpdateProfilePicture
{
    public class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, ErrorOr<ProfilePictureResponseDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;

        public UpdateProfilePictureCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService,
            IMapper mapper)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        public async Task<ErrorOr<ProfilePictureResponseDTO>> Handle(
            UpdateProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            // Get the user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Error.NotFound("auth.user.not.found", "User not found.");
            }

            // Validate file exists
            if (request.ProfilePictureFile == null || request.ProfilePictureFile.Length == 0)
            {
                return Error.Validation("image.empty", "No file was provided.");
            }

            // Store the old image URL for potential cleanup
            var oldProfilePictureUrl = user.ProfilePictureUrl;

            try
            {
                // Upload new image to Cloudinary
                using (var stream = request.ProfilePictureFile.OpenReadStream())
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(
                        stream,
                        request.ProfilePictureFile.FileName);

                    if (uploadResult.IsError)
                    {
                        return uploadResult.Errors;
                    }

                    // Update user with new image URL
                    user.ProfilePictureUrl = uploadResult.Value.ImageUrl;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        // If database update fails, attempt to delete the uploaded image
                        await _cloudinaryService.DeleteImageAsync(uploadResult.Value.PublicId);
                        return Error.Failure("image.update.failed", "Failed to update user profile picture.");
                    }

                    // Delete old image from Cloudinary if it existed
                    if (!string.IsNullOrEmpty(oldProfilePictureUrl))
                    {
                        // Extract public ID from old URL
                        var oldPublicId = ExtractPublicIdFromUrl(oldProfilePictureUrl);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(oldPublicId);
                        }
                    }

                    return new ProfilePictureResponseDTO
                    {
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Message = "Profile picture updated successfully."
                    };
                }
            }
            catch (Exception ex)
            {
                return Error.Failure("image.upload.exception", $"An error occurred while uploading: {ex.Message}");
            }
        }

        private string? ExtractPublicIdFromUrl(string imageUrl)
        {
            // Cloudinary URL format: https://res.cloudinary.com/{cloud_name}/image/upload/{public_id}.{ext}
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.Segments;

                // Find the "upload" segment and get the next segment
                var uploadIndex = Array.FindIndex(segments, s => s.Contains("upload", StringComparison.OrdinalIgnoreCase));
                if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
                {
                    var lastSegment = segments[uploadIndex + 1].TrimEnd('/');
                    // Remove version prefix (v1234567890) if present
                    if (lastSegment.StartsWith("v"))
                    {
                        lastSegment = lastSegment.Substring(lastSegment.IndexOf('/') + 1);
                    }
                    // Remove file extension
                    var publicId = Path.GetFileNameWithoutExtension(lastSegment);
                    return publicId;
                }
            }
            catch
            {
                // If extraction fails, return null - old image will not be deleted
            }

            return null;
        }
    }
}