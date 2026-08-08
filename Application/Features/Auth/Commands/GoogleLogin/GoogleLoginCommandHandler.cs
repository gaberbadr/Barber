using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Application.Interfaces;
using Error = ErrorOr.Error;
using Domain.Entities;
using Domain.Repositories;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ErrorOr<TokenResponseDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly ILogger<GoogleLoginCommandHandler> _logger;

        public GoogleLoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            ILogger<GoogleLoginCommandHandler> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ErrorOr<TokenResponseDTO>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get external login info from Google
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return Error.Failure("auth.google.info.not.found", "External login info not found. Please complete Google login first.");
                }

                // Try to find user by Google login
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (user == null)
                {
                    // Get email from Google claims
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    if (string.IsNullOrEmpty(email))
                    {
                        return Error.Failure("auth.google.email.not.provided", "Email not provided by Google.");
                    }

                    // Find user by email
                    user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        // Create new user
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                            IsActive = true
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            return Error.Failure("auth.google.user.creation.failed", "Failed to create user account.");
                        }
                    }
                    else if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }

                    // Link Google login to user
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        return Error.Failure("auth.google.login.link.failed", "Failed to link Google account.");
                    }
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return Error.Unauthorized("auth.user.blocked", "This account is blocked. Please contact support.");
                }

                // Generate tokens
                var tokenResponse = await GenerateTokenResponseAsync(user, request.IpAddress);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Google login failed: {Message}", ex.Message);
                return Error.Failure("auth.google.login.failed", $"Google login failed: {ex.Message}");
            }
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