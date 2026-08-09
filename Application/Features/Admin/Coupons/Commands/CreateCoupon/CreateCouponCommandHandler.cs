using Application.Features.Coupons.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Coupons.Commands.CreateCoupon
{
    public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, ErrorOr<CouponDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCouponCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<CouponDTO>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
        {
            var couponRepo = _unitOfWork.Repository<Coupon, int>();

            // Check for duplicate coupon code
            var existingCoupon = await couponRepo.FindOneAsync(c => c.Code == request.Code.ToUpper());
            if (existingCoupon != null)
                return Error.Conflict("coupon.code.exists", "A coupon with this code already exists.");

            var coupon = new Coupon
            {
                Code = request.Code.ToUpper(),
                DiscountPercentage = request.DiscountPercentage,
                StartDate = request.StartDate,
                ExpiryDate = request.ExpiryDate,
                IsActive = true,
                UsageLimit = request.UsageLimit,
                TimesUsed = 0,
                CreatedAt = DateTime.UtcNow
            };

            await couponRepo.AddAsync(coupon);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<CouponDTO>(coupon);
            return result;
        }
    }
}