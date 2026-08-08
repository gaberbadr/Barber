using ErrorOr;

namespace Application.Interfaces
{
    public interface ICloudinaryService
    {

        /// Uploads an image to Cloudinary and returns the public URL and public ID.
        Task<ErrorOr<CloudinaryUploadResult>> UploadImageAsync(Stream imageStream, string fileName);

        /// Deletes an image from Cloudinary by its public ID.
        Task<ErrorOr<Success>> DeleteImageAsync(string publicId);
    }

    /// Result of a successful Cloudinary upload operation.
    public class CloudinaryUploadResult
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
    }
}