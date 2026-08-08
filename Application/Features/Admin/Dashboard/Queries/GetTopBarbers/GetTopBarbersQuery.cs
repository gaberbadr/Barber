using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetTopBarbers
{
    public class GetTopBarbersQuery : IRequest<ErrorOr<List<TopBarberDTO>>>
    {
        public int Count { get; set; } = 10;
    }
}