using Application.Features.Services.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;

namespace Application.Features.Services.Queries.GetAll
{
    public class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, ErrorOr<List<ServiceDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllServicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ErrorOr<List<ServiceDTO>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
        {
            var serviceRepo = _unitOfWork.Repository<Service, int>();
            var services = await serviceRepo.FindAsync(s => s.IsActive && !s.IsDeleted);

            var dtos = _mapper.Map<List<ServiceDTO>>(services.OrderBy(s => s.Name).ToList());
            return dtos;
        }
    }
}