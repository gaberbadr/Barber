using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQuery : IRequest<ErrorOr<DashboardStatsDTO>>
    {
    }
}