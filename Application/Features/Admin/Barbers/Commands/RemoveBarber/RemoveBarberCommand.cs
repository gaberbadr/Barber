using MediatR;
using ErrorOr;

namespace Application.Features.Admin.Barbers.Commands.RemoveBarber
{
    public class RemoveBarberCommand : IRequest<ErrorOr<Success>>
    {
        public string BarberId { get; set; } = string.Empty;
    }
}