using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Bookings.Commands.UpdateStatus
{
    public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, ErrorOr<BookingDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;

        public UpdateBookingStatusCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<BookingDTO>> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var booking = await bookingRepo.GetAsync(request.BookingId);

            if (booking == null)
                return Error.NotFound("booking.not.found", "الحجز ده مش موجود.");

            var requestor = await _userManager.FindByIdAsync(request.RequestingUserId);
            if (requestor == null)
                return Error.NotFound("user.not.found", "المستخدم ده مش موجود.");

            var isAdmin = await _userManager.IsInRoleAsync(requestor, "Admin");
            var isAssignedBarber = booking.BarberId == request.RequestingUserId;

            if (!isAdmin && !isAssignedBarber)
                return Error.Forbidden("booking.not.authorized", "مينفعش تغير حالة حجز مش بتاعك.");

            if (request.NewStatus == BookingStatus.Cancelled)
                return Error.Validation("booking.status.invalid", "مينفعش تلغي الحجز من خلال الخاصية دي.");

            if (request.NewStatus != BookingStatus.Arrived && request.NewStatus != BookingStatus.DidNotArrive)
                return Error.Validation("booking.status.invalid", "الحالة دي مش مسموح بيها.");

            if (booking.Status != BookingStatus.Confirmed)
                return Error.Validation("booking.status.invalid", "مينفعش تغير حالة الحجز إلا لو كان مؤكد.");

            var localNow = _timeProvider.GetLocalNow();
            var bookingDate = booking.BookingDate;
            var today = DateOnly.FromDateTime(localNow.Date);

            if (today != bookingDate)
                return Error.Validation("booking.date.invalid", "مينفعش تغير حالة حجز غير حجز النهارده بس.");

            booking.Status = request.NewStatus;
            booking.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

            bookingRepo.Update(booking);
            await _unitOfWork.CompleteAsync();

            var customer = await _userManager.FindByIdAsync(booking.CustomerId);
            var barber = await _userManager.FindByIdAsync(booking.BarberId);

            var dto = _mapper.Map<BookingDTO>(booking);
            dto.CustomerName = customer?.FullName ?? "";
            dto.BarberName = barber?.FullName ?? "";

            return dto;
        }
    }
}
