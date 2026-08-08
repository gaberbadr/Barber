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
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Register CurrentUser helper to extract user info from HttpContext
            services.AddScoped(sp => new CurrentUser(sp.GetRequiredService<IHttpContextAccessor>().HttpContext!));

            return services;
        }
    }
}