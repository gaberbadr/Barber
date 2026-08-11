using Application.Features.Admin.Dashboard.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Admin.Dashboard.Queries.GetShopWorkingHours
{
    public class GetShopWorkingHoursQuery : IRequest<ErrorOr<List<ShopWorkingHourDTO>>>
    {
    }
}
