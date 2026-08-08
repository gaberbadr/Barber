using MediatR;
using ErrorOr;
using Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Auth.Commands.UpdateProfilePicture
{
    public class UpdateProfilePictureCommand : IRequest<ErrorOr<ProfilePictureResponseDTO>>
    {
        public string UserId { get; set; } = string.Empty;
        public IFormFile? ProfilePictureFile { get; set; }
    }
}