using Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Commands.ToggleUserBlock
{
    public class ToggleUserBlockCommandHandler : IRequestHandler<ToggleUserBlockCommand, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ToggleUserBlockCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<Success>> Handle(ToggleUserBlockCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Error.NotFound("user.not.found", "المستخدم ده مش موجود.");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
                return Error.Forbidden("admin.cannot.be.blocked", "مينفعش تحظر أدمن.");

            user.IsActive = !request.Block;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result.Success;
        }
    }
}