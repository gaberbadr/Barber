using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Infrastructure.Persistence.Context;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure Barber role exists
            if (!await roleManager.RoleExistsAsync("Barber"))
                await roleManager.CreateAsync(new IdentityRole("Barber"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Seed barbers
            if (!userManager.Users.Any(u => u.UserName!.StartsWith("barber")))
            {
                var barbers = new[]
                {
                    new { Name = "Ahmed Hassan", Email = "barber1@barbershop.com", Phone = "01000000001" },
                    new { Name = "Mohamed Ali", Email = "barber2@barbershop.com", Phone = "01000000002" },
                    new { Name = "Omar Ibrahim", Email = "barber3@barbershop.com", Phone = "01000000003" }
                };

                foreach (var b in barbers)
                {
                    var barber = new ApplicationUser
                    {
                        UserName = b.Email,
                        Email = b.Email,
                        FullName = b.Name,
                        PhoneNumber = b.Phone,
                        EmailConfirmed = true,
                        IsActive = true,
                        BookingDurationMinutes = 30,
                        AcceptingBookings = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(barber, "Barber@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(barber, "Barber");

                        // Add default working hours (Sat-Thu 9:00-22:00, Fri closed)
                        for (int day = 0; day < 7; day++)
                        {
                            var dayOfWeek = (DayOfWeek)day;
                            context.BarberWorkingHours.Add(new BarberWorkingHour
                            {
                                BarberId = barber.Id,
                                DayOfWeek = dayOfWeek,
                                OpeningTime = new TimeOnly(9, 0),
                                ClosingTime = new TimeOnly(22, 0),
                                IsClosed = dayOfWeek == DayOfWeek.Friday,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            // Seed services
            if (!context.Services.Any())
            {
                var services = new[]
                {
                    new Service { Name = "Hair", Description = "Classic haircut", Price = 100m, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Service { Name = "Beard", Description = "Beard trim and shape", Price = 50m, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Service { Name = "Hair Wash", Description = "Hair wash and conditioning", Price = 30m, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Service { Name = "Hot Towel", Description = "Hot towel treatment", Price = 40m, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Service { Name = "Kids Haircut", Description = "Haircut for children under 12", Price = 60m, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Service { Name = "Styling", Description = "Hair styling and blowout", Price = 80m, IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                context.Services.AddRange(services);
            }

            // Seed shop working hours
            if (!context.ShopWorkingHours.Any())
            {
                for (int day = 0; day < 7; day++)
                {
                    var dayOfWeek = (DayOfWeek)day;
                    context.ShopWorkingHours.Add(new ShopWorkingHour
                    {
                        DayOfWeek = dayOfWeek,
                        OpeningTime = new TimeOnly(9, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        IsClosed = dayOfWeek == DayOfWeek.Friday,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Seed coupons
            if (!context.Coupons.Any())
            {
                context.Coupons.AddRange(
                    new Coupon
                    {
                        Code = "SAVE10",
                        DiscountPercentage = 10m,
                        StartDate = DateTime.UtcNow.AddDays(-30),
                        ExpiryDate = DateTime.UtcNow.AddDays(90),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Coupon
                    {
                        Code = "SAVE20",
                        DiscountPercentage = 20m,
                        StartDate = DateTime.UtcNow.AddDays(-30),
                        ExpiryDate = DateTime.UtcNow.AddDays(60),
                        IsActive = true,
                        UsageLimit = 50,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            }

            // Seed global booking settings
            if (!context.GlobalBookingSettings.Any())
            {
                context.GlobalBookingSettings.Add(new GlobalBookingSettings
                {
                    MaximumBookingAdvanceDays = 7,
                    CancellationWindowHours = 16,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
