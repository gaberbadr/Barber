using MediatR;
using ErrorOr;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<ErrorOr<TokenResponseDTO>>
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}