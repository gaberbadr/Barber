using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Application.Features.Auth.Commands.GoogleLogin;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.SendOTP;
using Application.Features.Auth.Commands.VerifyOTP;
using Application.Features.Auth.Commands.UpdateProfilePicture;
using Application.Features.Auth.Commands.DeleteProfilePicture;
using API.Controllers;
using Domain.Entities;
using Requests.Auth;
using Application.Features.Auth.DTOs;
using Application.Common.Models;
using API.Helpers;

namespace Zero.Controllers
{
    /// <summary>
    /// API endpoints for authentication operations.
    /// Reuses existing authentication infrastructure from Application and Infrastructure layers.
    /// </summary>
    public class AuthController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly CurrentUser _currentUser;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, CurrentUser currentUser, ILogger<AuthController> logger ,SignInManager<ApplicationUser> signInManager)
        {
            _mediator = mediator;
            _currentUser = currentUser;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Sends an OTP verification code to the provided email address.
        /// </summary>
        /// <param name="request">Email address to send OTP to</param>
        /// <returns>Success or failure message</returns>
        [HttpPost("send-verification-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new SendOTPCommand { Email = request.Email };
            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Verification code sent successfully." }));
        }

        /// <summary>
        /// Verifies the OTP code and signs the user in with JWT and refresh tokens.
        /// </summary>
        /// <param name="request">Email and OTP code</param>
        /// <returns>Access token, refresh token, and expiration details</returns>
        [HttpPost("verify-code")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new VerifyOTPCommand
            {
                Email = request.Email,
                Code = request.Code,
                IpAddress = GetClientIpAddress()
            };

            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        // ========== Google Login ==========
        [HttpGet("google-login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin([FromQuery] string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return BadRequest("Return URL is required.");

            if (!IsValidReturnUrl(returnUrl))
                return BadRequest("Invalid return URL.");

            var redirectUrl = Url.Action(
                nameof(GoogleCallback),"Auth",new { returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme,redirectUrl!);

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// Handles the callback from Google OAuth2 authentication.
        /// Creates or authenticates the user and generates tokens.
        /// <returns>Redirect to frontend with tokens in query parameters or error</returns>
        [HttpGet("google-callback")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleCallback([FromQuery] string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !IsValidReturnUrl(returnUrl))
            {
                return BadRequest("Invalid return URL.");
            }

            var authenticateResult =
                await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!authenticateResult.Succeeded)
            {
                return Redirect(
                    $"{returnUrl}?error={Uri.EscapeDataString("Google authentication failed.")}");
            }

            var command = new GoogleLoginCommand
            {
                IpAddress = GetClientIpAddress()
            };

            var result = await _mediator.Send(command);

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            if (result.IsError)
            {
                return Redirect(
                    $"{returnUrl}?error={Uri.EscapeDataString(result.FirstError.Description)}");
            }

            var token = result.Value;

            var callbackUrl =
                $"{returnUrl}" +
                $"?accessToken={Uri.EscapeDataString(token.AccessToken)}" +
                $"&refreshToken={Uri.EscapeDataString(token.RefreshToken)}" +
                $"&accessTokenExpiresAt={Uri.EscapeDataString(token.AccessTokenExpiresAt.ToString("O"))}" +
                $"&refreshTokenExpiresAt={Uri.EscapeDataString(token.RefreshTokenExpiresAt.ToString("O"))}";

            return Redirect(callbackUrl);
        }

        /// <summary>
        /// Refreshes the access token using a valid refresh token.
        /// Rotates the refresh token if applicable.
        /// </summary>
        /// <param name="request">Current refresh token</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken,
                IpAddress = GetClientIpAddress()
            };

            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        // Revokes a refresh token to invalidate it.
        [HttpPost("revoke-refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeRefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new LogoutCommand
            {
                UserId = _currentUser.UserId,
                RefreshToken = request.RefreshToken
            };

            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Refresh token revoked successfully." }));
        }

        // Logs out the authenticated user by revoking all refresh tokens.
        // Requires JWT authentication.
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            var command = new LogoutCommand { UserId = _currentUser.UserId };
            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Logout successful." }));
        }

        /// <summary>
        /// Uploads or updates the authenticated user's profile picture.
        /// Replaces existing profile picture if present.
        /// Requires JWT authentication.
        /// </summary>
        /// <param name="request">Profile picture file (max 5MB, formats: jpg, jpeg, png, gif, webp)</param>
        /// <returns>Updated profile picture URL</returns>
        [Authorize]
        [HttpPut("me/profile-picture")]
        [ProducesResponseType(typeof(ProfilePictureResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureRequest request)
        {
            if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            {
                return Unauthorized(new { message = "User must be authenticated." });
            }

            if (request?.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            var command = new UpdateProfilePictureCommand
            {
                UserId = _currentUser.UserId,
                ProfilePictureFile = request.File
            };

            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Deletes the authenticated user's profile picture.
        /// Requires JWT authentication.
        /// </summary>
        /// <returns>Success message</returns>
        [Authorize]
        [HttpDelete("me/profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            {
                return Unauthorized(new { message = "User must be authenticated." });
            }

            var command = new DeleteProfilePictureCommand
            {
                UserId = _currentUser.UserId
            };

            var result = await _mediator.Send(command);

            if (result.IsError)
            {
                return HandleErrorResult(result.Errors);
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Profile picture deleted successfully." }));
        }



        // Gets the client's IP address from the request.
        private string? GetClientIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString().Split(',').First();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        //it is validate the returnUrl.
        private bool IsValidReturnUrl(string returnUrl)
        {
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            var allowedBaseUrls = configuration
                .GetSection("Frontend:AllowedBaseUrls")
                .Get<string[]>() ?? Array.Empty<string>();

            return allowedBaseUrls.Any(baseUrl =>
                returnUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase));
        }
    }
}



/*
 Frontend (signin.html)
        ?
        ? GET /api/Auth/google-login
        ?
        ? Query:
        ? returnUrl=http://127.0.0.1:5500/callback.html
        ?
Backend (GoogleLogin)
        ?
        ? Creates Google authentication challenge
        ?
        ? Stores the returnUrl inside the callback URL
        ?
Google OAuth
        ?
        ? User selects a Google account
        ?
        ? 
        ?
Google
        ?
        ? Redirects to:
        ? /api/Auth/google-callback?returnUrl=http://127.0.0.1:5500/callback.html
        ?
Backend (GoogleCallback)
        ?
        ? Reads Google user information
        ?
        ? Creates or signs in the local user
        ?
        ? Generates Access Token + Refresh Token
        ?
Backend
        ?
        ? Redirects to:
        ? http://127.0.0.1:5500/callback.html
        ? ?accessToken=...
        ? &refreshToken=...
        ?
Frontend (callback.html)
        ?
        ? Reads tokens from query string
        ?
        ? Saves them in localStorage
 */