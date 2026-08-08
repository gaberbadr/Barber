using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Barbers.Commands.UpdateWorkingHours
{
    public class UpdateWorkingHoursCommandHandler : IRequestHandler<UpdateWorkingHoursCommand, ErrorOr<BarberDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateWorkingHoursCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<BarberDTO>> Handle(UpdateWorkingHoursCommand request, CancellationToken cancellationToken)
        {
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "Barber not found.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Forbidden("barber.not.barber", "User is not a barber.");

            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();

            // Remove existing working hours for this barber
            var existingHours = await workingHoursRepo.FindAsync(w => w.BarberId == request.BarberId);
            foreach (var existing in existingHours)
            {
                workingHoursRepo.Delete(existing);
            }

            // Add new working hours
            var newHours = request.WorkingHours.Select(w => new BarberWorkingHour
            {
                BarberId = request.BarberId,
                DayOfWeek = w.DayOfWeek,
                OpeningTime = w.OpeningTime,
                ClosingTime = w.ClosingTime,
                IsClosed = w.IsClosed,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await workingHoursRepo.AddRangeAsync(newHours);
            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<BarberDTO>(barber);
            dto.WorkingHours = newHours.Select(w => new BarberWorkingHourDTO
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