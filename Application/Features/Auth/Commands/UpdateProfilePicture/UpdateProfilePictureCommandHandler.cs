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
                return Error.NotFound("auth.user.not.found", "المستخدم ده مش موجود.");
            }

            // Validate file exists
            if (request.ProfilePictureFile == null || request.ProfilePictureFile.Length == 0)
            {
                return Error.Validation("image.empty", "مفيش ملف اتبعت.");
            }

            // Store the old image URL and public ID for potential cleanup
            var oldProfilePictureUrl = user.ProfilePictureUrl;
            var oldProfilePicturePublicId = user.ProfilePicturePublicId;

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

                    // Update user with new image URL and public ID
                    user.ProfilePictureUrl = uploadResult.Value.ImageUrl;
                    user.ProfilePicturePublicId = uploadResult.Value.PublicId;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        // If database update fails, attempt to delete the uploaded image
                        await _cloudinaryService.DeleteImageAsync(uploadResult.Value.PublicId);
                        return Error.Failure("image.update.failed", "فشل تحديث صورة الملف الشخصي.");
                    }

                    // Delete old image from Cloudinary if it existed
                    if (!string.IsNullOrEmpty(oldProfilePicturePublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(oldProfilePicturePublicId);
                    }

                    return new ProfilePictureResponseDTO
                    {
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Message = "تم تحديث صورة الملف الشخصي بنجاح."
                    };
                }
            }
            catch (Exception ex)
            {
                return Error.Failure("image.upload.exception", $"حصل مشكلة أثناء الرفع: {ex.Message}");
            }
        }


    }
}