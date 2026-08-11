using Application.Features.Services.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Admin.Services.Commands.CreateService
{
    public class CreateServiceCommand : IRequest<ErrorOr<ServiceDTO>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
