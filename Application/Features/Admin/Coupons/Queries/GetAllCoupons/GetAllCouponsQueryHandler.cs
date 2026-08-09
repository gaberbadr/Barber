using Application.Common.Pagination;
using Application.Features.Coupons.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Coupons.Queries.GetAllCoupons
{
    public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, ErrorOr<PaginationResponse<CouponDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllCouponsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<PaginationResponse<CouponDTO>>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
        {
            var couponRepo = _unitOfWork.Repository<Coupon, int>();

            var query = couponRepo.GetIQueryable();

            if (request.IsActive.HasValue)
                query = query.Where(c => c.IsActive == request.IsActive.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var coupons = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<CouponDTO>>(coupons);
            return new PaginationResponse<CouponDTO>(request.PageSize, request.PageIndex, totalCount, result);
        }
    }
}