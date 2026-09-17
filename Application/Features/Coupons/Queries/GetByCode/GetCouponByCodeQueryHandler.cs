using Application.Features.Coupons.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Error = ErrorOr.Error;

namespace Application.Features.Coupons.Queries.GetByCode
{
    public class GetCouponByCodeQueryHandler : IRequestHandler<GetCouponByCodeQuery, ErrorOr<CouponValueDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCouponByCodeQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<CouponValueDTO>> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
        {
            var code = request.Code.Trim().ToUpperInvariant();
            var coupon = await _unitOfWork.Repository<Coupon, int>()
                .FindOneAsync(c => c.Code == code, cancellationToken);

            var now = DateTime.UtcNow;
            if (coupon is null ||
                !coupon.IsActive ||
                coupon.StartDate > now ||
                coupon.ExpiryDate < now ||
                (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value))
            {
                return Error.NotFound("coupon.not_found", "??? ????? ??? ????? ?? ??? ????.");
            }

            return new CouponValueDTO
            {
                Code = coupon.Code,
                DiscountPercentage = coupon.DiscountPercentage
            };
        }
    }
}
