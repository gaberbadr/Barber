using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;
using Domain.Enums;

namespace Application.Features.Barbers.Commands.UpdateWorkingHours
{
    public class UpdateWorkingHoursCommandHandler : IRequestHandler<UpdateWorkingHoursCommand, ErrorOr<BarberDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;

        public UpdateWorkingHoursCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TimeProvider timeProvider)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<BarberDTO>> Handle(UpdateWorkingHoursCommand request, CancellationToken cancellationToken)
        {
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "الحلاق ده مش موجود.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Forbidden("barber.not.barber", "المستخدم ده مش حلاق.");

            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
            var upcomingBookings = await bookingRepo.FindAsync(b => 
                b.BarberId == request.BarberId && 
                b.BookingDate >= today && 
                b.Status == BookingStatus.Confirmed);

            var existingHours = await workingHoursRepo.FindAsync(w => w.BarberId == request.BarberId);
            
            var shopHoursRepo = _unitOfWork.Repository<ShopWorkingHour, int>();
            var allShopHours = await shopHoursRepo.GetAllAsync();

            // Validate that we are not changing hours for a day with upcoming bookings,
            // and that barber hours do not exceed shop hours.
            foreach (var reqHour in request.WorkingHours)
            {
                // Verify against global shop hours
                var shopHour = allShopHours.FirstOrDefault(s => s.DayOfWeek == reqHour.DayOfWeek);
                if (!reqHour.IsClosed && shopHour != null && !shopHour.IsClosed)
                {
                    if (reqHour.OpeningTime < shopHour.OpeningTime || reqHour.ClosingTime > shopHour.ClosingTime)
                    {
                        return Error.Validation("workinghours.outside.shop", 
                            $"مينفعش تحدد مواعيد عمل بره مواعيد المحل الأساسية ({shopHour.OpeningTime} - {shopHour.ClosingTime}) ليوم {reqHour.DayOfWeek}. يرجى التواصل مع الإدارة.");
                    }
                }

                var existing = existingHours.FirstOrDefault(h => h.DayOfWeek == reqHour.DayOfWeek);
                bool isChanged = false;
                
                if (existing == null) 
                    isChanged = true;
                else if (existing.OpeningTime != reqHour.OpeningTime || 
                         existing.ClosingTime != reqHour.ClosingTime || 
                         existing.IsClosed != reqHour.IsClosed) 
                    isChanged = true;

                if (isChanged)
                {
                    var hasBookings = upcomingBookings.Any(b => b.BookingDate.DayOfWeek == reqHour.DayOfWeek);
                    if (hasBookings)
                    {
                        return Error.Conflict("workinghours.conflict", $"مينفعش تغيّر مواعيد العمل ليوم {reqHour.DayOfWeek} عشان فيه حجوزات جاية في اليوم ده.");
                    }
                }
            }

            // Remove existing working hours for this barber
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