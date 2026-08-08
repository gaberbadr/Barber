using Microsoft.Extensions.Configuration;

namespace API.Extensions
{
    /// <summary>
    /// Extension methods for configuring CORS policies.
    /// </summary>
    public static class CorsExtensions
    {
        /// <summary>
        /// Adds CORS policy allowing frontend origins from appsettings.
        /// </summary>
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var allowedUrls = configuration
                .GetSection("Frontend:AllowedBaseUrls")
                .Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    if (allowedUrls.Length > 0)
                    {
                        policy.WithOrigins(allowedUrls)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    }
                    else
                    {
                        // Fallback if no URLs configured
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                });
            });

            return services;
        }
    }
}