using MediatR;
using ErrorOr;

namespace Application.Features.Admin.Dashboard.Commands.ToggleUserBlock
{
    public class ToggleUserBlockCommand : IRequest<ErrorOr<Success>>
    {
        public string UserId { get; set; } = string.Empty;
        public bool Block { get; set; }
    }
}