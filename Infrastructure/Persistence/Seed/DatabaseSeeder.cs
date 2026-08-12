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
        public static async Task SeedAsync(ApplicationDbContext context)
        {
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
