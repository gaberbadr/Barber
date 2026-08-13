using System.Security.Claims;

namespace API.Helpers
{
    /// <summary>
    /// Helper class to extract and expose current user information from HttpContext.
    /// Safely handles cases where HttpContext may be null (outside request context).
    /// </summary>
    public class CurrentUser
    {
        private readonly HttpContext? _httpContext;

        public CurrentUser(HttpContext? httpContext)
        {
            _httpContext = httpContext;
        }

        /// <summary>
        /// Gets the current user's ID from JWT claims.
        /// Returns null if HttpContext is unavailable or user is not authenticated.
        /// </summary>
        public string? UserId
        {
            get
            {
                if (_httpContext?.User == null)
                    return null;
                    
                var userIdClaim = _httpContext.User.FindFirstValue("uid") 
                    ?? _httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return userIdClaim;
            }
        }

        /// <summary>
        /// Gets the current user's email from JWT claims.
        /// Returns null if HttpContext is unavailable or user is not authenticated.
        /// </summary>
        public string? Email
        {
            get => _httpContext?.User?.FindFirstValue(ClaimTypes.Email);
        }

        /// <summary>
        /// Gets the current user's name from JWT claims.
        /// Returns null if HttpContext is unavailable or user is not authenticated.
        /// </summary>
        public string? Name
        {
            get => _httpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        /// <summary>
        /// Determines if the user is authenticated.
        /// Returns false if HttpContext is unavailable.
        /// </summary>
        public bool IsAuthenticated => _httpContext?.User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Gets the user's IP address.
        /// Returns null if HttpContext is unavailable.
        /// </summary>
        public string? IpAddress
        {
            get => _httpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}