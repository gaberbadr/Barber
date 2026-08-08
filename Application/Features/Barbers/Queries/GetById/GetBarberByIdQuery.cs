using MediatR;
using ErrorOr;
using Application.Features.Barbers.DTOs;

namespace Application.Features.Barbers.Queries.GetById
{
    public class GetBarberByIdQuery : IRequest<ErrorOr<BarberDTO>>
    {
        public string BarberId { get; set; } = string.Empty;
    }
}