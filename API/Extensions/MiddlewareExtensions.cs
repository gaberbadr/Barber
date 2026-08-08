using API.Middleware;
using Microsoft.Extensions.Options;

namespace API.Extensions
{
    /// <summary>
    /// Extension methods for configuring request middleware pipeline.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Configures the HTTP request pipeline with all necessary middleware.
        /// </summary>
        public static WebApplication UseApiMiddlewares(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Production exception handling - don't expose detailed errors
                app.UseExceptionHandler("/error");
            }

            app.UseHttpsRedirection();
            
            // Security headers for production
            if (!app.Environment.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Append("X-Frame-Options", "DENY");
                    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                    await next();
                });
            }

            app.UseCors("AllowFrontend");
            
            // Rate limiting middleware should be early in the pipeline, before authentication
            // to protect against brute force and bot attacks
            app.UseMiddleware<RateLimitMiddleware>();
            
            app.UseAuthentication();

            // Check if user is active (not blocked) after authentication
            app.UseMiddleware<UserActiveStatusMiddleware>();
            
            app.UseMiddleware<ExceptionMiddleware>();
            
            app.UseAuthorization();

            return app;
        }
    }
}