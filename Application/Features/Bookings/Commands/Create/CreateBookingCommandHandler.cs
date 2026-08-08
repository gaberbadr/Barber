using Application.Features.Bookings.DTOs;
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

        public CreateBookingCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ErrorOr<BookingDTO>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify customer exists and is active
            var customer = await _userManager.FindByIdAsync(request.CustomerId);
            if (customer == null)
                return Error.NotFound("booking.customer.not.found", "Customer not found.");

            if (!customer.IsActive)
                return Error.Failure("booking.customer.blocked", "Your account is blocked. Please contact support.");

            // 2. Validate FullName and PhoneNumber
            if (string.IsNullOrWhiteSpace(customer.FullName))
                return Error.Validation("booking.customer.fullname.required", "Full name is required to make a booking.");

            if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
                return Error.Validation("booking.customer.phone.required", "Phone number is required to make a booking.");

            // 3. Verify barber exists, is active, and is a Barber
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("booking.barber.not.found", "Barber not found.");

            if (!barber.IsActive)
                return Error.Failure("booking.barber.inactive", "This barber is currently inactive.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Validation("booking.barber.not.barber", "The selected user is not a barber.");

            // 4. Verify barber accepts bookings
            if (!barber.AcceptingBookings)
                return Error.Failure("booking.barber.not.accepting", "This barber is not currently accepting bookings.");

            // 5. Get global settings
            var settingsRepo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await settingsRepo.GetAllAsync()).FirstOrDefault();
            if (settings == null)
                return Error.Failure("booking.settings.missing", "Booking settings not configured.");

            // 6. Verify date is not past and within advance booking window
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (request.BookingDate < today)
                return Error.Validation("booking.date.past", "Cannot book in the past.");

            var maxDate = today.AddDays(settings.MaximumBookingAdvanceDays);
            if (request.BookingDate > maxDate)
                return Error.Validation("booking.date.too.far",
                    $"Bookings can only be made up to {settings.MaximumBookingAdvanceDays} days in advance.");

            // 7. Verify shop is open on this day
            var shopHoursRepo = _unitOfWork.Repository<ShopWorkingHour, int>();
            var shopHours = await shopHoursRepo.FindFirstAsync(
                s => s.DayOfWeek == request.BookingDate.DayOfWeek);

            if (shopHours == null || shopHours.IsClosed)
                return Error.Validation("booking.shop.closed", "The shop is closed on this day.");

            // 8. Verify barber is working on this day
            var barberHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var barberHours = await barberHoursRepo.FindFirstAsync(
                b => b.BarberId == request.BarberId && b.DayOfWeek == request.BookingDate.DayOfWeek);

            if (barberHours == null || barberHours.IsClosed)
                return Error.Validation("booking.barber.not.working", "The barber is not working on this day.");

            // 9. Verify slot matches barber's booking duration
            var endTime = request.StartTime.AddMinutes(barber.BookingDurationMinutes);

            // Effective opening = max(shop, barber), Effective closing = min(shop, barber)
            var effectiveOpening = shopHours.OpeningTime > barberHours.OpeningTime
                ? shopHours.OpeningTime : barberHours.OpeningTime;
            var effectiveClosing = shopHours.ClosingTime < barberHours.ClosingTime
                ? shopHours.ClosingTime : barberHours.ClosingTime;

            if (request.StartTime < effectiveOpening || endTime > effectiveClosing)
                return Error.Validation("booking.slot.outside.hours",
                    "The selected time slot is outside working hours.");

            // Verify slot aligns to duration grid from opening
            var minutesFromOpen = (request.StartTime.ToTimeSpan() - effectiveOpening.ToTimeSpan()).TotalMinutes;
            if (minutesFromOpen % barber.BookingDurationMinutes != 0)
                return Error.Validation("booking.slot.invalid",
                    $"Time slots must align to {barber.BookingDurationMinutes}-minute intervals.");

            // 10. Check for overlapping confirmed bookings (concurrency-safe check)
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var overlapping = await bookingRepo.FindFirstAsync(b =>
                b.BarberId == request.BarberId &&
                b.BookingDate == request.BookingDate &&
                b.Status == BookingStatus.Confirmed &&
                b.StartTime < endTime &&
                b.EndTime > request.StartTime);

            if (overlapping != null)
                return Error.Conflict("booking.slot.unavailable", "This time slot is already booked.");

            // 11. Check user daily booking rule
            var existingUserBooking = await bookingRepo.FindFirstAsync(b =>
                b.CustomerId == request.CustomerId &&
                b.BookingDate == request.BookingDate &&
                b.Status == BookingStatus.Confirmed);

            if (existingUserBooking != null)
                return Error.Conflict("booking.daily.limit", "You already have a confirmed booking for this day.");

            // 12. Validate selected services
            var serviceRepo = _unitOfWork.Repository<Service, int>();
            var services = new List<Service>();
            foreach (var serviceId in request.ServiceIds)
            {
                var service = await serviceRepo.GetAsync(serviceId);
                if (service == null || !service.IsActive)
                    return Error.Validation("booking.service.invalid",
                        $"Service with ID {serviceId} is not available.");
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
                    c.Code == request.CouponCode &&
                    c.IsActive &&
                    !c.IsDeleted);

                if (coupon == null)
                    return Error.Validation("booking.coupon.invalid", "Invalid coupon code.");

                var now = DateTime.UtcNow;
                if (now < coupon.StartDate || now > coupon.ExpiryDate)
                    return Error.Validation("booking.coupon.expired", "This coupon has expired.");

                if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
                    return Error.Validation("booking.coupon.limit.reached", "This coupon has reached its usage limit.");

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

            await _unitOfWork.CompleteAsync();

            // Map and return
            var dto = _mapper.Map<BookingDTO>(booking);
            dto.CustomerName = customer.FullName;
            dto.BarberName = barber.FullName;
            dto.Items = _mapper.Map<List<BookingItemDTO>>(items);
            return dto;
        }
    }
}