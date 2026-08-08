using MediatR;
using ErrorOr;
using Application.Features.Barbers.DTOs;

namespace Application.Features.Barbers.Queries.GetAll
{
    public class GetAllBarbersQuery : IRequest<ErrorOr<List<BarberDTO>>>
    {
    }
}