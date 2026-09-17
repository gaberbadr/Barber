using Application.Features.Services.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
            var services = await serviceRepo.GetIQueryable()
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ServiceDTO>>(services);
            return dtos;
        }
    }
}
