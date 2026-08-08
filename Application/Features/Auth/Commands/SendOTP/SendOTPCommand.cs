using MediatR;
using ErrorOr;

namespace Application.Features.Auth.Commands.SendOTP
{
    public class SendOTPCommand : IRequest<ErrorOr<Success>>
    {
        public string Email { get; set; } = string.Empty;
    }
}