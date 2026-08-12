using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Commands.UpdateSettings
{
    public class UpdateGlobalSettingsCommandHandler : IRequestHandler<UpdateGlobalSettingsCommand, ErrorOr<GlobalSettingsDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateGlobalSettingsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<GlobalSettingsDTO>> Handle(UpdateGlobalSettingsCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await repo.GetAllAsync()).FirstOrDefault();

            if (settings == null)
                return Error.NotFound("settings.not.found", "الإعدادات العامة مش مظبوطة.");

            settings.MaximumBookingAdvanceDays = request.MaximumBookingAdvanceDays;
            settings.CancellationWindowHours = request.CancellationWindowHours;
            settings.UpdatedAt = DateTime.UtcNow;

            repo.Update(settings);
            await _unitOfWork.CompleteAsync();

            return new GlobalSettingsDTO
            {
                Id = settings.Id,
                MaximumBookingAdvanceDays = settings.MaximumBookingAdvanceDays,
                CancellationWindowHours = settings.CancellationWindowHours
            };
        }
    }
}