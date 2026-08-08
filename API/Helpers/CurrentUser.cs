using System.Security.Claims;

namespace API.Helpers
{
    /// <summary>
    /// Helper class to extract and expose current user information from HttpContext.
    /// </summary>
    public class CurrentUser
    {
        private readonly HttpContext _httpContext;

        public CurrentUser(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        /// <summary>
        /// Gets the current user's ID from JWT claims.
        /// </summary>
        public string? UserId
        {
            get
            {
                var userIdClaim = _httpContext.User.FindFirstValue("uid") 
                    ?? _httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return userIdClaim;
            }
        }

        /// <summary>
        /// Gets the current user's email from JWT claims.
        /// </summary>
        public string? Email
        {
            get => _httpContext.User.FindFirstValue(ClaimTypes.Email);
        }

        /// <summary>
        /// Gets the current user's name from JWT claims.
        /// </summary>
        public string? Name
        {
            get => _httpContext.User.FindFirstValue(ClaimTypes.Name);
        }

        /// <summary>
        /// Determines if the user is authenticated.
        /// </summary>
        public bool IsAuthenticated => _httpContext.User.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Gets the user's IP address.
        /// </summary>
        public string? IpAddress
        {
            get => _httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}