using Application.Features.Services.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Services.Queries.GetAll
{
    public class GetAllServicesQueryMappingProfile : Profile
    {
        public GetAllServicesQueryMappingProfile()
        {
            CreateMap<Service, ServiceDTO>();
        }
    }
}