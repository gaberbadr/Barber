using MediatR;
using ErrorOr;
using Application.Features.Services.DTOs;

namespace Application.Features.Services.Queries.GetAll
{
    public class GetAllServicesQuery : IRequest<ErrorOr<List<ServiceDTO>>>
    {
    }
}