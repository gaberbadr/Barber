using MediatR;
using ErrorOr;

namespace Application.Features.Auth.Commands.DeleteProfilePicture
{
    public class DeleteProfilePictureCommand : IRequest<ErrorOr<Success>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}