using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ErrorOr;
using Infrastructure.Storage.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Error = ErrorOr.Error;


namespace Infrastructure.Storage.Services
{
    /// Service for handling image uploads and deletions to Cloudinary.
    /// Implements ICloudinaryService interface for dependency injection.
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryOptions _settings;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinaryOptions> options, ILogger<CloudinaryService> logger)
        {
            if (options?.Value == null)
            {
                throw new ArgumentNullException(nameof(options), "CloudinaryOptions cannot be null");
            }

            _settings = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Validate configuration
            _settings.Validate();

            // Initialize Cloudinary client
            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            _cloudinary = new Cloudinary(account);

            _logger.LogInformation("CloudinaryService initialized with CloudName: {CloudName}", _settings.CloudName);
        }

        public async Task<ErrorOr<CloudinaryUploadResult>> UploadImageAsync(Stream imageStream, string fileName)
        {
            if (imageStream == null)
            {
                _logger.LogWarning("Attempted to upload image with null stream");
                return Error.Failure("image.upload.failed", "ملف الصورة مينفعش يكون فاضي");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("Attempted to upload image with invalid filename");
                return Error.Failure("image.upload.failed", "اسم الملف مينفعش يكون فاضي");
            }

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, imageStream),
                    Folder = "barber-app-images"
                };

                _logger.LogInformation("Uploading image: {FileName}", fileName);
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Image upload failed for {FileName}: {ErrorMessage}",
                        fileName, uploadResult.Error.Message);
                    return Error.Failure("image.upload.failed", uploadResult.Error.Message);
                }

                var secureUrl = uploadResult.SecureUrl.ToString();
                var publicId = uploadResult.PublicId;

                _logger.LogInformation("Image uploaded successfully: {FileName} -> {Url}", fileName, secureUrl);

                return new CloudinaryUploadResult
                {
                    ImageUrl = secureUrl,
                    PublicId = publicId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while uploading image: {FileName}", fileName);
                return Error.Failure("image.upload.failed", $"حصل مشكلة: {ex.Message}");
            }
        }

        public async Task<ErrorOr<Success>> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                _logger.LogWarning("Attempted to delete image with invalid public ID");
                return Error.Failure("image.delete.failed", "معرف الصورة مينفعش يكون فاضي");
            }

            try
            {
                _logger.LogInformation("Deleting image with public ID: {PublicId}", publicId);
                var deleteResult = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

                if (deleteResult.Error != null)
                {
                    _logger.LogError("Image deletion failed for public ID {PublicId}: {ErrorMessage}",
                        publicId, deleteResult.Error.Message);
                    return Error.Failure("image.delete.failed", deleteResult.Error.Message);
                }

                _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);
                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting image with public ID: {PublicId}", publicId);
                return Error.Failure("image.delete.failed", $"حصل مشكلة: {ex.Message}");
            }
        }
    }
}
