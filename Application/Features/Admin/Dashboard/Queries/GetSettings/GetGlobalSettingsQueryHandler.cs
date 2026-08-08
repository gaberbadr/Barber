using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Queries.GetSettings
{
    public class GetGlobalSettingsQueryHandler : IRequestHandler<GetGlobalSettingsQuery, ErrorOr<GlobalSettingsDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetGlobalSettingsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<GlobalSettingsDTO>> Handle(GetGlobalSettingsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await repo.GetAllAsync()).FirstOrDefault();

            if (settings == null)
                return Error.NotFound("settings.not.found", "Global settings not configured.");

            return new GlobalSettingsDTO
            {
                Id = settings.Id,
                MaximumBookingAdvanceDays = settings.MaximumBookingAdvanceDays,
                CancellationWindowHours = settings.CancellationWindowHours
            };
        }
    }
}