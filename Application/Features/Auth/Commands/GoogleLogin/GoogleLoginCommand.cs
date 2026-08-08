using MediatR;
using ErrorOr;
using Application.Features.Auth.DTOs;


namespace Application.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommand : IRequest<ErrorOr<TokenResponseDTO>>
    {
        public string? IpAddress { get; set; }
    }
}