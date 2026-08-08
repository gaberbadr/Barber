using Application.Features.Auth.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Auth.Commands.VerifyOTP
{
    public class VerifyOTPCommandHandler : IRequestHandler<VerifyOTPCommand, ErrorOr<TokenResponseDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public VerifyOTPCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<ErrorOr<TokenResponseDTO>> Handle(VerifyOTPCommand request, CancellationToken cancellationToken)
        {
            // Find user
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Error.NotFound("auth.user.not.found", "User not found.");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Error.Failure("auth.user.blocked", "This account is blocked. Please contact support.");
            }

            // Check if OTP is expired
            if (user.CodeExpiresAt == null || user.CodeExpiresAt < DateTime.UtcNow)
            {
                return Error.Failure("auth.otp.expired", "Verification code has expired.");
            }

            // Verify OTP
            if (user.VerificationCode != request.Code)
            {
                return Error.Failure("auth.otp.invalid", "Invalid verification code.");
            }

            // Mark email as confirmed and clear OTP
            user.EmailConfirmed = true;
            user.VerificationCode = null;
            user.CodeExpiresAt = null;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Error.Failure("auth.otp.verification.failed", "Failed to verify OTP.");
            }

            // Generate tokens
            var tokenResponse = await GenerateTokenResponseAsync(user, request.IpAddress);
            return tokenResponse;
        }

        private async Task<TokenResponseDTO> GenerateTokenResponseAsync(ApplicationUser user, string? ipAddress)
        {
            var (accessToken, accessExp) = await _jwtService.GenerateAccessTokenAsync(user, _userManager);

            var refreshTokenRepo = _unitOfWork.Repository<RefreshTokenTable, int>();
            var existingTokens = await refreshTokenRepo.FindAsync(t =>
                t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
            var existingToken = existingTokens.OrderByDescending(t => t.ExpiresAt).FirstOrDefault();

            string refreshToken;
            DateTime refreshExp;

            if (existingToken != null)
            {
                refreshToken = existingToken.Token;
                refreshExp = existingToken.ExpiresAt;
            }
            else
            {
                (refreshToken, refreshExp) = _jwtService.GenerateRefreshToken();
                await refreshTokenRepo.AddAsync(new RefreshTokenTable
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = refreshExp,
                    CreatedByIp = ipAddress
                });
                await _unitOfWork.CompleteAsync();
            }

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExp,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExp
            };
        }
    }
}