using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetTopServices
{
    public class GetTopServicesQuery : IRequest<ErrorOr<List<TopServiceDTO>>>
    {
        public int Count { get; set; } = 10;
    }
}