using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;

namespace API.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        // Existing DbSets
        public DbSet<RefreshTokenTable> RefreshTokens { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        // New DbSets
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<ShopWorkingHour> ShopWorkingHours { get; set; }
        public DbSet<BarberWorkingHour> BarberWorkingHours { get; set; }
        public DbSet<GlobalBookingSettings> GlobalBookingSettings { get; set; }
    }
}
