using System.Security.Claims;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Middleware
{
    /// <summary>
    /// Middleware to check if the authenticated user is active (not blocked by admin).
    /// </summary>
    public class UserActiveStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActiveStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager)
        {
            // Only check if user is authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);

                    // Check if user exists and is not active (blocked by admin)
                    if (user != null && !user.IsActive)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            message = "Your account has been blocked by the administrator. Please contact the support team for assistance.",
                            code = "USER_BLOCKED"
                        };

                        await context.Response.WriteAsJsonAsync(response);
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}