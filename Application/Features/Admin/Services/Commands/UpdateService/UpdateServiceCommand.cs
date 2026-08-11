using Application.Features.Services.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Admin.Services.Commands.UpdateService
{
    public class UpdateServiceCommand : IRequest<ErrorOr<ServiceDTO>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
