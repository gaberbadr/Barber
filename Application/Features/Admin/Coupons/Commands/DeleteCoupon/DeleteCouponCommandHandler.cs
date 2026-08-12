using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Coupons.Commands.DeleteCoupon
{
    public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, ErrorOr<Success>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCouponCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
        {
            var couponRepo = _unitOfWork.Repository<Coupon, int>();

            var coupon = await couponRepo.GetByIdAsync(request.CouponId);
            if (coupon == null)
                return Error.NotFound("coupon.not.found", "كود الخصم ده مش موجود.");

            // Bookings with CouponId will be set to NULL due to OnDelete(DeleteBehavior.SetNull)
            // CouponCodeSnapshot is preserved for historical booking records
            couponRepo.Delete(coupon);
            await _unitOfWork.CompleteAsync();

            return Result.Success;
        }
    }
}