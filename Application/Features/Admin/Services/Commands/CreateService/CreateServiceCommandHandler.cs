using Application.Features.Services.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Services.Commands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ErrorOr<ServiceDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public CreateServiceCommandHandler(IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<ServiceDTO>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            var serviceRepo = _unitOfWork.Repository<Service, int>();

            var service = new Service
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                IsActive = request.IsActive,
                CreatedAt = _timeProvider.GetUtcNow().DateTime
            };

            await serviceRepo.AddAsync(service);
            await _unitOfWork.CompleteAsync();

            return new ServiceDTO
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt
            };
        }
    }
}
