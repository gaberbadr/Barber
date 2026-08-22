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
            {
                var roles = await _userManager.GetRolesAsync(existingUser);
                
                if (roles.Contains("Barber"))
                {
                    return Error.Conflict("barber.already.exists", "المستخدم ده متسجل كحلاق بالفعل.");
                }
                
                if (roles.Contains("Admin"))
                {
                    return Error.Validation("barber.admin.cannot.be.barber", "مينفعش ترقي أدمن كحلاق.");
                }

                // User exists - upgrade to barber role
                return await UpgradeUserToBarberAsync(existingUser, request);
            }

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

            // Create default working hours
            await CreateDefaultWorkingHoursAsync(barber.Id);

            var result = _mapper.Map<BarberDTO>(barber);
            var workingHours = await GetBarberWorkingHoursAsync(barber.Id);
            result.WorkingHours = workingHours;

            return result;
        }

        private async Task<ErrorOr<BarberDTO>> UpgradeUserToBarberAsync(ApplicationUser user, AddBarberCommand request)
        {
            // Check if user is already a barber
            var isAlreadyBarber = await _userManager.IsInRoleAsync(user, "Barber");
            if (isAlreadyBarber)
                return Error.Conflict("barber.already.exists", "المستخدم ده متسجل كحلاق بالفعل.");

            // Update user profile with barber information
            user.FullName = request.FullName;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;
            
            user.BookingDurationMinutes = request.BookingDurationMinutes;
            user.AcceptingBookings = request.AcceptingBookings;
            user.UpdatedAt = DateTime.UtcNow;

            // Update the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errorMessages = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Error.Failure("barber.update.failed", errorMessages);
            }

            // Assign Barber role
            var roleResult = await _userManager.AddToRoleAsync(user, "Barber");
            if (!roleResult.Succeeded)
            {
                var errorMessages = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Error.Failure("barber.role.assignment.failed", errorMessages);
            }

            // Check if working hours already exist
            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var existingWorkingHours = (await workingHoursRepo.FindAsync(w => w.BarberId == user.Id)).ToList();
            
            if (existingWorkingHours.Count == 0)
            {
                // Create default working hours only if they don't exist
                await CreateDefaultWorkingHoursAsync(user.Id);
            }

            var result = _mapper.Map<BarberDTO>(user);
            var workingHours = await GetBarberWorkingHoursAsync(user.Id);
            result.WorkingHours = workingHours;

            return result;
        }

        private async Task CreateDefaultWorkingHoursAsync(string barberId)
        {
            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            for (int day = 0; day < 7; day++)
            {
                var dayOfWeek = (DayOfWeek)day;
                var workingHour = new BarberWorkingHour
                {
                    BarberId = barberId,
                    DayOfWeek = dayOfWeek,
                    OpeningTime = new TimeOnly(9, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    IsClosed = dayOfWeek == DayOfWeek.Friday,
                    CreatedAt = DateTime.UtcNow
                };
                await workingHoursRepo.AddAsync(workingHour);
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task<List<BarberWorkingHourDTO>> GetBarberWorkingHoursAsync(string barberId)
        {
            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var workingHours = await workingHoursRepo.FindAsync(w => w.BarberId == barberId);
            
            return workingHours.Select(w => new BarberWorkingHourDTO
            {
                Id = w.Id,
                DayOfWeek = w.DayOfWeek,
                DayName = w.DayOfWeek.ToString(),
                OpeningTime = w.OpeningTime,
                ClosingTime = w.ClosingTime,
                IsClosed = w.IsClosed
            }).OrderBy(w => w.DayOfWeek).ToList();
        }
    }
}