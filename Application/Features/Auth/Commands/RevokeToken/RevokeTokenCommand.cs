using MediatR;
using ErrorOr;

namespace Application.Features.Auth.Commands.RevokeToken
{
    public class RevokeTokenCommand : IRequest<ErrorOr<Success>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}