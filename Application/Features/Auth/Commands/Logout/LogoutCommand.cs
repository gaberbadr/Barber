using MediatR;
using ErrorOr;

namespace Application.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<ErrorOr<Success>>
    {
        public string UserId { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
    }
}