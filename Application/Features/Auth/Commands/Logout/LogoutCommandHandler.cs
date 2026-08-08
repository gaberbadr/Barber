using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;

namespace Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ErrorOr<Success>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Success>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshTokenRepo = _unitOfWork.Repository<RefreshTokenTable, int>();

            // Delete all refresh tokens for the user
            await refreshTokenRepo.DeleteRangeAsync(t => t.UserId == request.UserId);
            await _unitOfWork.CompleteAsync();

            return Result.Success;
        }
    }
}