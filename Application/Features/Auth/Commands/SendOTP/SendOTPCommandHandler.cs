using Application.Interfaces;
using Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Auth.Commands.SendOTP
{
    public class SendOTPCommandHandler : IRequestHandler<SendOTPCommand, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailConfiguration _emailConfiguration;

        public SendOTPCommandHandler(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IEmailConfiguration emailConfiguration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailConfiguration = emailConfiguration;
        }

        public async Task<ErrorOr<Success>> Handle(SendOTPCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find or create user
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = request.Email,
                        UserName = request.Email,
                        EmailConfirmed = false
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return Error.Failure("auth.user.creation.failed", "Failed to create user account.");
                    }
                }

                // Check if user is blocked
                if (!user.IsActive)
                {
                    return Error.Failure("auth.user.blocked", "This account is blocked. Please contact support.");
                }

                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                user.VerificationCode = otp;
                user.CodeExpiresAt = DateTime.UtcNow.AddMinutes(_emailConfiguration.ExpirationMinutes);

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return Error.Failure("auth.otp.generation.failed", "Failed to generate OTP.");
                }

                var userName = !string.IsNullOrWhiteSpace(user.FullName)
                ? user.FullName
                : request.Email.Split('@')[0];

                // Send OTP via email (uses ExpirationMinutes from appsettings)
                await _emailSender.SendOtpEmailAsync(
                    request.Email,
                    userName,
                    otp);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure("auth.otp.send.failed", $"Failed to send OTP: {ex.Message}");
            }
        }
    }
}