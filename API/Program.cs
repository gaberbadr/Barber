using API.DependencyInjection;
using API.Extensions;
using API.Infrastructure.Persistence.Context;
using Application;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Add Services
// ==========================================

// API layer services
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddApiLayerServices();

// Application, Infrastructure, and API layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices();

// Identity, Authentication, and Authorization
builder.Services.AddIdentityServices();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddAuthorizationServices();

// ==========================================
// Build and Configure Middleware
// ==========================================

var app = builder.Build();

// Apply database migrations and seed initial data
// This runs on startup to ensure database is initialized
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<ApplicationDbContext>();
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
var logger = loggerFactory.CreateLogger("Startup.DatabaseInitialization");

try
{
    logger.LogInformation("Starting database migration...");
    await context.Database.MigrateAsync();
    logger.LogInformation("Database migration completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(
        ex,
        "Failed to apply database migrations. " +
        "Application startup will continue but database may not be properly initialized. " +
        "Ensure database connectivity and migrations are applied manually if needed.");
}

try
{
    logger.LogInformation("Starting role seeding...");
    await DefaultRolesSeeder.SeedRolesAsync(roleManager);
    logger.LogInformation("Role seeding completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(
        ex,
        "Failed to seed default roles. " +
        "Application startup will continue but required roles may not exist. " +
        "Authorization may fail for some users.");
}

try
{
    logger.LogInformation("Starting admin user seeding...");
    await DefaultAdminSeeder.SeedAppUserAsync(userManager, roleManager);
    logger.LogInformation("Admin user seeding completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(
        ex,
        "Failed to seed default admin users. " +
        "Application startup will continue but default admin accounts may not be created. " +
        "You may need to create admin users manually.");
}

try
{
    logger.LogInformation("Starting database seeding...");
    await DatabaseSeeder.SeedAsync(context);
    logger.LogInformation("Database seeding completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(
        ex,
        "Failed to seed database with initial data. " +
        "Application startup will continue but some reference data may be missing. " +
        "Services, working hours, coupons, and booking settings may need to be created manually.");
}

logger.LogInformation("Database initialization completed. Application is starting.");

app.UseSwaggerDocumentation();
app.UseApiMiddlewares();
app.MapControllers();

app.Run();

// Add-Migration InitialCreate -OutputDir Persistence\Migrations