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

//when app run, it automaticlly apply all migrations (Update DataBase)
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<ApplicationDbContext>();
var LoggerFactory = services.GetRequiredService<ILoggerFactory>();
var usermanger = services.GetRequiredService<UserManager<ApplicationUser>>();
var userrole = services.GetRequiredService<RoleManager<IdentityRole>>();
try
{
    await context.Database.MigrateAsync();// Update-DataBase
    await DefaultRolesSeeder.SeedRolesAsync(userrole);
    await DefaultAdminSeeder.SeedAppUserAsync(usermanger, userrole);
    await DatabaseSeeder.SeedAsync(context, usermanger, userrole);
}
catch (Exception ex)
{
    var logger = LoggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "their are problem during migration");
}

app.UseSwaggerDocumentation();
app.UseApiMiddlewares();
app.MapControllers();

app.Run();