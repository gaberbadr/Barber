using Microsoft.Extensions.DependencyInjection;
using API.Helpers;

namespace API.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering API-specific services.
    /// </summary>
    public static class ApiServiceRegistration
    {
        /// <summary>
        /// Adds API-specific services to the dependency injection container.
        /// CurrentUser is registered as scoped and will be resolved during request context,
        /// allowing safe access to the current HttpContext.
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Register CurrentUser helper to extract user info from HttpContext.
            // The HttpContext accessor is resolved at request time, ensuring HttpContext is available.
            // CurrentUser safely handles null HttpContext scenarios.
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                // HttpContext is null outside of request processing, which is handled safely by CurrentUser
                return new CurrentUser(httpContextAccessor.HttpContext);
            });

            return services;
        }
    }
}