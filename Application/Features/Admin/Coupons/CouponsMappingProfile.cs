using Application.Features.Coupons.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Admin.Coupons
{
    public class CouponsMappingProfile : Profile
    {
        public CouponsMappingProfile()
        {
            CreateMap<Coupon, CouponDTO>();
        }
    }
}