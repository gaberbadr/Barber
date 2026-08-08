using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetSettings
{
    public class GetGlobalSettingsQuery : IRequest<ErrorOr<GlobalSettingsDTO>>
    {
    }
}