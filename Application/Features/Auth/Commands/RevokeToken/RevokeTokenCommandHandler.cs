using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;

using Error = ErrorOr.Error;

namespace Application.Features.Auth.Commands.RevokeToken
{
    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, ErrorOr<Success>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevokeTokenCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Success>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshTokenRepo = _unitOfWork.Repository<RefreshTokenTable, int>();
            var tokens = await refreshTokenRepo.FindAsync(t => t.Token == request.RefreshToken);
            var tokenEntry = tokens.FirstOrDefault();

            if (tokenEntry == null)
            {
                return Error.NotFound("auth.token.not.found", "Refresh token not found.");
            }

            tokenEntry.RevokedAt = DateTime.UtcNow;
            refreshTokenRepo.Update(tokenEntry);
            await _unitOfWork.CompleteAsync();

            return Result.Success;
        }
    }
}