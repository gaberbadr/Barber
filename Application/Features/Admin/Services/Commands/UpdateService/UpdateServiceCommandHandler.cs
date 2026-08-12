using Application.Features.Services.DTOs;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Services.Commands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ErrorOr<ServiceDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public UpdateServiceCommandHandler(IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<ServiceDTO>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            var serviceRepo = _unitOfWork.Repository<Service, int>();

            var service = await serviceRepo.GetByIdAsync(request.Id);
            if (service == null)
            {
                return Error.NotFound("service.not.found", "الخدمة دي مش موجودة.");
            }

            service.Name = request.Name;
            service.Description = request.Description;
            service.Price = request.Price;
            service.IsActive = request.IsActive;
            service.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

            serviceRepo.Update(service);
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
