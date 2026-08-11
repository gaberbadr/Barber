using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Dashboard.Queries.GetShopWorkingHours
{
    public class GetShopWorkingHoursQueryHandler : IRequestHandler<GetShopWorkingHoursQuery, ErrorOr<List<ShopWorkingHourDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetShopWorkingHoursQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<List<ShopWorkingHourDTO>>> Handle(GetShopWorkingHoursQuery request, CancellationToken cancellationToken)
        {
            var shopHoursRepo = _unitOfWork.Repository<ShopWorkingHour, int>();
            var shopHours = await shopHoursRepo.GetAllAsync();

            var dtos = shopHours.Select(w => new ShopWorkingHourDTO
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
