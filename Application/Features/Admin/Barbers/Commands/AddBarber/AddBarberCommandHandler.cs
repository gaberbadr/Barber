using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Barbers.Commands.AddBarber
{
    public class AddBarberCommandHandler : IRequestHandler<AddBarberCommand, ErrorOr<BarberDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddBarberCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<BarberDTO>> Handle(AddBarberCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Error.Conflict("barber.email.exists", "Email already in use.");

            // Create new barber user
            var barber = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                BookingDurationMinutes = request.BookingDurationMinutes,
                AcceptingBookings = request.AcceptingBookings,
                CreatedAt = DateTime.UtcNow
            };

            // Create user without password - barber will use Google Sign-in
            var createResult = await _userManager.CreateAsync(barber);
            if (!createResult.Succeeded)
            {
                var errorMessages = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Error.Failure("barber.creation.failed", errorMessages);
            }

            // Assign Barber role
            var roleResult = await _userManager.AddToRoleAsync(barber, "Barber");
            if (!roleResult.Succeeded)
            {
                var errorMessages = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Error.Failure("barber.role.assignment.failed", errorMessages);
            }

            // Create default working hours (Sat-Thu 9:00-22:00, Fri closed)
            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            for (int day = 0; day < 7; day++)
            {
                var dayOfWeek = (DayOfWeek)day;
                var workingHour = new BarberWorkingHour
                {
                    BarberId = barber.Id,
                    DayOfWeek = dayOfWeek,
                    OpeningTime = new TimeOnly(9, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    IsClosed = dayOfWeek == DayOfWeek.Friday,
                    CreatedAt = DateTime.UtcNow
                };
                await workingHoursRepo.AddAsync(workingHour);
            }

            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<BarberDTO>(barber);
            var workingHours = await workingHoursRepo.FindAsync(w => w.BarberId == barber.Id);
            result.WorkingHours = workingHours.Select(w => new BarberWorkingHourDTO
            {
                Id = w.Id,
                DayOfWeek = w.DayOfWeek,
                DayName = w.DayOfWeek.ToString(),
                OpeningTime = w.OpeningTime,
                ClosingTime = w.ClosingTime,
                IsClosed = w.IsClosed
            }).OrderBy(w => w.DayOfWeek).ToList();

            return result;
        }
    }
}