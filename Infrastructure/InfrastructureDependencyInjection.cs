using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Infrastructure.Persistence.Context;
using Application.Interfaces;
using Domain.Repositories;
using Infrastructure.Authentication;
using Infrastructure.Caching.Options;
using Infrastructure.Email;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Storage.Options;
using Infrastructure.Storage.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Time;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Zero.Infrastructure.Caching.Services;
using Zero.Infrastructure.Services;

namespace Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddDatabaseContext(services, configuration);

            AddUnitOfWork(services);

            AddCachingServices(services, configuration);

            // Register TimeProvider
            services.AddSingleton<TimeProvider, EgyptTimeProvider>();

            AddStorageServices(services, configuration);

            AddEmailServices(services, configuration);

            AddApplicationServices(services, configuration);

            return services;
        }

        private static void AddDatabaseContext(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "DefaultConnection is not configured.");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        private static void AddUnitOfWork(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void AddCachingServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var redisOptions =
                configuration.GetSection("CacheSettings:Redis").Get<RedisOptions>()
                ?? new RedisOptions();

            services.Configure<RedisOptions>(
                configuration.GetSection("CacheSettings:Redis"));

            if (!redisOptions.Enabled)
                return;

            var connectionString =
                configuration.GetConnectionString("REDIS_URL");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "ConnectionStrings:REDIS_URL was not found.");

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var uri = new Uri(connectionString);

                var options = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    Ssl = true,
                    ConnectRetry = 3,
                    ConnectTimeout = redisOptions.ConnectTimeoutMs,
                    SyncTimeout = redisOptions.SyncTimeoutMs,
                    KeepAlive = 180
                };

                options.EndPoints.Add(uri.Host, uri.Port);

                var userInfo = uri.UserInfo.Split(':', 2);

                options.User = userInfo[0];
                options.Password = userInfo[1];

                // Upstash Recommended
                options.AbortOnConnectFail = false;
                options.Ssl = true;

                options.ConnectRetry = 3;
                options.ConnectTimeout = redisOptions.ConnectTimeoutMs;
                options.SyncTimeout = redisOptions.SyncTimeoutMs;
                options.KeepAlive = 180;

                return ConnectionMultiplexer.Connect(options);
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        private static void AddStorageServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var cloudinaryOptions =
                configuration.GetSection("CloudinarySettings")
                             .Get<CloudinaryOptions>();

            if (cloudinaryOptions == null ||
                string.IsNullOrWhiteSpace(cloudinaryOptions.CloudName))
            {
                return;
            }

            services.Configure<CloudinaryOptions>(
                configuration.GetSection("CloudinarySettings"));

            services.AddScoped<ICloudinaryService, CloudinaryService>();
        }

        private static void AddEmailServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var emailOptions = configuration.GetSection("Email").Get<EmailOptions>();

            if (emailOptions == null || string.IsNullOrWhiteSpace(emailOptions.SenderEmail))
            {
                return;
            }

            services.Configure<EmailOptions>(configuration.GetSection("Email"));

            services.AddScoped<IEmailConfiguration>(provider => 
                provider.GetRequiredService<IOptions<EmailOptions>>().Value);

            services.AddScoped<IEmailSender, EmailService>();
        }

        private static void AddApplicationServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection("JWT"));
            
            services.AddScoped<IJwtService, JwtService>();

            services.Configure<RateLimitingOptions>(
                configuration.GetSection("RateLimiting"));
            services.AddScoped<IRateLimitService, RateLimitService>();
        }
    }
}
