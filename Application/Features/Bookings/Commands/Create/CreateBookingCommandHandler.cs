using Application.Features.Bookings.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Commands.Create
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ErrorOr<BookingDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;
        private readonly IEmailSender _emailSender;

        public CreateBookingCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            TimeProvider timeProvider,
            IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _timeProvider = timeProvider;
            _emailSender = emailSender;
        }

        public async Task<ErrorOr<BookingDTO>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify customer exists and is active
            var customer = await _userManager.FindByIdAsync(request.CustomerId);
            if (customer == null)
                return Error.NotFound("booking.customer.not.found", "العميل مش موجود.");

            if (!customer.IsActive)
                return Error.Failure("booking.customer.blocked", "حسابك موقوف. يرجى التواصل مع الدعم.");

            // 2. Validate FullName and PhoneNumber provided by customer
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Error.Validation("booking.fullname.required", "الاسم بالكامل مطلوب عشان تحجز.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return Error.Validation("booking.phone.required", "رقم الموبايل مطلوب عشان تحجز.");

            // 3. Verify barber exists, is active, and is a Barber
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("booking.barber.not.found", "الحلاق ده مش موجود.");

            if (!barber.IsActive)
                return Error.Failure("booking.barber.inactive", "الحلاق ده غير مفعل دلوقتي.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Validation("booking.barber.not.barber", "المستخدم اللي اخترته مش حلاق.");

            // 4. Verify barber accepts bookings
            if (!barber.AcceptingBookings)
                return Error.Failure("booking.barber.not.accepting", "الحلاق ده مش بيستقبل حجوزات دلوقتي.");

            // 5. Get global settings
            var settingsRepo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await settingsRepo.GetAllAsync()).FirstOrDefault();
            if (settings == null)
                return Error.Failure("booking.settings.missing", "إعدادات الحجز مش مظبوطة.");

            // 6. Verify date is not past and within advance booking window
            var nowDateTime = _timeProvider.GetLocalNow().DateTime;
            var today = DateOnly.FromDateTime(nowDateTime);
            
            if (request.BookingDate < today)
                return Error.Validation("booking.date.past", "مينفعش تحجز في الماضي.");
                
            if (request.BookingDate == today)
            {
                var nowTime = TimeOnly.FromDateTime(nowDateTime);
                if (request.StartTime < nowTime)
                    return Error.Validation("booking.time.past", "مينفعش تحجز ميعاد فات.");
            }

            var maxDate = today.AddDays(settings.MaximumBookingAdvanceDays);
            if (request.BookingDate > maxDate)
                return Error.Validation("booking.date.too.far",
                    $"الحجز متاح لحد {settings.MaximumBookingAdvanceDays} أيام قدام بس.");

            // 7. Verify shop is open on this day
            var shopHoursRepo = _unitOfWork.Repository<ShopWorkingHour, int>();
            var shopHours = await shopHoursRepo.FindFirstAsync(
                s => s.DayOfWeek == request.BookingDate.DayOfWeek);

            if (shopHours == null || shopHours.IsClosed)
                return Error.Validation("booking.shop.closed", "المحل قافل في اليوم ده.");

            // 8. Verify barber is working on this day
            var barberHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var barberHours = await barberHoursRepo.FindFirstAsync(
                b => b.BarberId == request.BarberId && b.DayOfWeek == request.BookingDate.DayOfWeek);

            if (barberHours == null || barberHours.IsClosed)
                return Error.Validation("booking.barber.not.working", "الحلاق مش شغال في اليوم ده.");

            var endTime = request.StartTime.AddMinutes(barber.BookingDurationMinutes);
            bool isWrapped = endTime < request.StartTime;

            // Effective opening = max(shop, barber), Effective closing = min(shop, barber)
            var effectiveOpening = shopHours.OpeningTime > barberHours.OpeningTime
                ? shopHours.OpeningTime : barberHours.OpeningTime;
            var effectiveClosing = shopHours.ClosingTime < barberHours.ClosingTime
                ? shopHours.ClosingTime : barberHours.ClosingTime;

            if (request.StartTime < effectiveOpening || endTime > effectiveClosing || isWrapped)
                return Error.Validation("booking.slot.outside.hours",
                    "الميعاد اللي اخترته بره مواعيد العمل.");

            // Verify slot aligns to duration grid from opening
            var minutesFromOpen = (request.StartTime.ToTimeSpan() - effectiveOpening.ToTimeSpan()).TotalMinutes;
            if (minutesFromOpen < 0 || minutesFromOpen % barber.BookingDurationMinutes != 0)
                return Error.Validation("booking.slot.invalid",
                    $"المواعيد لازم تكون متقسمة لفترات مدتها {barber.BookingDurationMinutes} دقيقة.");

            // 10. Check for overlapping confirmed bookings (concurrency-safe check)
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var overlapping = await bookingRepo.FindFirstAsync(b =>
                b.BarberId == request.BarberId &&
                b.BookingDate == request.BookingDate &&
                b.Status == BookingStatus.Confirmed &&
                b.StartTime < endTime &&
                b.EndTime > request.StartTime);

            if (overlapping != null)
                return Error.Conflict("booking.slot.unavailable", "الميعاد ده محجوز قبل كده.");

            // 11. Check user daily booking rule
            var existingUserBooking = await bookingRepo.FindFirstAsync(b =>
                b.CustomerId == request.CustomerId &&
                b.BookingDate == request.BookingDate &&
                b.Status == BookingStatus.Confirmed);

            if (existingUserBooking != null)
                return Error.Conflict("booking.daily.limit", "إنت عندك حجز مؤكد في اليوم ده بالفعل.");

            // 12. Validate selected services
            var serviceRepo = _unitOfWork.Repository<Service, int>();
            var services = new List<Service>();
            foreach (var serviceId in request.ServiceIds)
            {
                var service = await serviceRepo.GetAsync(serviceId);
                if (service == null || !service.IsActive)
                    return Error.Validation("booking.service.invalid",
                        $"الخدمة رقم {serviceId} مش متاحة.");
                services.Add(service);
            }

            // 13. Calculate subtotal
            var subTotal = services.Sum(s => s.Price);

            // 14. Validate and apply coupon
            decimal discount = 0;
            Coupon? coupon = null;
            string? couponCodeSnapshot = null;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var couponRepo = _unitOfWork.Repository<Coupon, int>();
                coupon = await couponRepo.FindFirstAsync(c =>
                    c.Code == request.CouponCode.ToUpper() &&
                    c.IsActive &&
                    !c.IsDeleted);

                if (coupon == null)
                    return Error.Validation("booking.coupon.invalid", "كود الخصم غير صحيح.");

                var now = _timeProvider.GetLocalNow().DateTime;
                if (now < coupon.StartDate || now > coupon.ExpiryDate)
                    return Error.Validation("booking.coupon.expired", "كود الخصم ده منتهي الصلاحية.");

                if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
                    return Error.Validation("booking.coupon.limit.reached", "كود الخصم ده وصل للحد الأقصى للاستخدام.");

                discount = subTotal * (coupon.DiscountPercentage / 100m);
                couponCodeSnapshot = coupon.Code;
            }

            var totalPrice = subTotal - discount;

            // 15. Create booking
            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                BarberId = request.BarberId,
                BookingDate = request.BookingDate,
                StartTime = request.StartTime,
                EndTime = endTime,
                SubTotal = subTotal,
                Discount = discount,
                TotalPrice = totalPrice,
                CouponId = coupon?.Id,
                CouponCodeSnapshot = couponCodeSnapshot,
                CustomerNameSnapshot = request.FullName,
                CustomerPhoneSnapshot = request.PhoneNumber,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            await bookingRepo.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            // 16. Create booking items with snapshots
            var bookingItemRepo = _unitOfWork.Repository<BookingItem, int>();
            var items = services.Select(s => new BookingItem
            {
                BookingId = booking.Id,
                ServiceId = s.Id,
                ServiceNameSnapshot = s.Name,
                UnitPrice = s.Price,
                Quantity = 1,
                TotalPrice = s.Price
            }).ToList();

            await bookingItemRepo.AddRangeAsync(items);

            // 17. Increment coupon usage
            if (coupon != null)
            {
                coupon.TimesUsed++;
                var couponRepo = _unitOfWork.Repository<Coupon, int>();
                couponRepo.Update(coupon);
            }

            // 18. Update customer's ApplicationUser with booking information
            customer.FullName = request.FullName;
            customer.PhoneNumber = request.PhoneNumber;
            customer.UpdatedAt = DateTime.UtcNow;
            
            var updateResult = await _userManager.UpdateAsync(customer);
            if (!updateResult.Succeeded)
            {
                return Error.Failure("booking.customer.update.failed",
                    "فشل تحديث بيانات العميل. الحجز تم بس الملف الشخصي متحدثش.");
            }

            await _unitOfWork.CompleteAsync();

            // 19. Map and return
            var dto = _mapper.Map<BookingDTO>(booking);
            dto.CustomerName = booking.CustomerNameSnapshot ?? request.FullName;
            dto.CustomerPhone = booking.CustomerPhoneSnapshot;
            dto.BarberName = barber.FullName;
            dto.Items = _mapper.Map<List<BookingItemDTO>>(items);

            // 20. Send booking notification email to barber
            if (!string.IsNullOrWhiteSpace(barber.Email))
            {
                await _emailSender.SendBookingInfoAsync(
                    barber.Email,
                    barber.FullName ?? "Barber",
                    request.FullName,
                    request.PhoneNumber,
                    request.BookingDate,
                    request.StartTime);
            }

            return dto;
        }
    }
}