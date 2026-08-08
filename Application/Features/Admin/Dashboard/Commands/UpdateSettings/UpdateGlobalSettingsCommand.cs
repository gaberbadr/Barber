using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Commands.UpdateSettings
{
    public class UpdateGlobalSettingsCommand : IRequest<ErrorOr<GlobalSettingsDTO>>
    {
        public int MaximumBookingAdvanceDays { get; set; }
        public int CancellationWindowHours { get; set; }
    }
}