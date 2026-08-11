using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Dashboard.Commands.UpdateShopWorkingHours
{
    public class UpdateShopWorkingHoursCommandHandler : IRequestHandler<UpdateShopWorkingHoursCommand, ErrorOr<List<ShopWorkingHourDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShopWorkingHoursCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<List<ShopWorkingHourDTO>>> Handle(UpdateShopWorkingHoursCommand request, CancellationToken cancellationToken)
        {
            var shopHoursRepo = _unitOfWork.Repository<ShopWorkingHour, int>();
            var existingHours = await shopHoursRepo.GetAllAsync();

            // We update existing records without deleting/adding to preserve IDs and architecture.
            foreach (var reqHour in request.WorkingHours)
            {
                var existing = existingHours.FirstOrDefault(h => h.DayOfWeek == reqHour.DayOfWeek);
                if (existing != null)
                {
                    existing.OpeningTime = reqHour.OpeningTime;
                    existing.ClosingTime = reqHour.ClosingTime;
                    existing.IsClosed = reqHour.IsClosed;
                    shopHoursRepo.Update(existing);
                }
            }

            await _unitOfWork.CompleteAsync();

            var dtos = existingHours.Select(w => new ShopWorkingHourDTO
            {
                Id = w.Id,
                DayOfWeek = w.DayOfWeek,
                DayName = w.DayOfWeek.ToString(),
                OpeningTime = w.OpeningTime,
                ClosingTime = w.ClosingTime,
                IsClosed = w.IsClosed
            }).OrderBy(w => w.DayOfWeek).ToList();

            return dtos;
        }
    }
}
