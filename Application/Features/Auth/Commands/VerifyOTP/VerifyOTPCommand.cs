using MediatR;
using ErrorOr;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Commands.VerifyOTP
{
    public class VerifyOTPCommand : IRequest<ErrorOr<TokenResponseDTO>>
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}