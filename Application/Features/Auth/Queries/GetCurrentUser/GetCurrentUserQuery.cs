using MediatR;
using ErrorOr;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<ErrorOr<UserDTO>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}