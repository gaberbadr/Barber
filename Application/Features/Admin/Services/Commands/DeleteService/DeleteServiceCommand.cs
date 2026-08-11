using ErrorOr;
using MediatR;

namespace Application.Features.Admin.Services.Commands.DeleteService
{
    public class DeleteServiceCommand : IRequest<ErrorOr<Success>>
    {
        public int ServiceId { get; set; }
    }
}
