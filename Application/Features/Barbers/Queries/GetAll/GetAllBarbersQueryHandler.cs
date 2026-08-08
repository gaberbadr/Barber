using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Barbers.Queries.GetAll
{
    public class GetAllBarbersQueryHandler : IRequestHandler<GetAllBarbersQuery, ErrorOr<List<BarberDTO>>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllBarbersQueryHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<List<BarberDTO>>> Handle(GetAllBarbersQuery request, CancellationToken cancellationToken)
        {
            var barbers = await _userManager.GetUsersInRoleAsync("Barber");
            var activeBarbers = barbers.Where(b => !b.IsDeleted).ToList();

            var dtos = new List<BarberDTO>();
            foreach (var barber in activeBarbers)
            {
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

                dtos.Add(dto);
            }

            return dtos;
        }
    }
}