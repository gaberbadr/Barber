using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<ErrorOr<List<AdminUserDTO>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}