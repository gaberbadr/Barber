using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;
using Application.Common.Pagination;

namespace Application.Features.Admin.Dashboard.Queries.GetAllUsers
{
    public class GetAllUsersQuery : PaginationRequest, IRequest<ErrorOr<PaginationResponse<AdminUserDTO>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }
}