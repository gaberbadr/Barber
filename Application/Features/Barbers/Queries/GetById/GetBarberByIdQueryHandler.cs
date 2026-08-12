using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Barbers.Queries.GetById
{
    public class GetBarberByIdQueryHandler : IRequestHandler<GetBarberByIdQuery, ErrorOr<BarberDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetBarberByIdQueryHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<BarberDTO>> Handle(GetBarberByIdQuery request, CancellationToken cancellationToken)
        {
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null || barber.IsDeleted)
                return Error.NotFound("barber.not.found", "الحلاق ده مش موجود.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.NotFound("barber.not.found", "الحلاق ده مش موجود.");

            var dto = _mapper.Map<BarberDTO>(barber);

            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var workingHours = await workingHoursRepo.FindAsync(w => w.BarberId == barber.Id);
            dto.WorkingHours = workingHours.Select(w => new BarberWorkingHourDTO
            {
                Id = w.Id,
                DayOfWeek = w.DayOfWeek,
                DayName = w.DayOfWeek.ToString(),
                OpeningTime = w.OpeningTime,
                ClosingTime = w.ClosingTime,
                IsClosed = w.IsClosed
            }).OrderBy(w => w.DayOfWeek).ToList();

            return dto;
        }
    }
}