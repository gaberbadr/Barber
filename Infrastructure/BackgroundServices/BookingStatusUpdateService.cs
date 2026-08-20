using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundServices
{
    public class BookingStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusUpdateService> _logger;
        private readonly TimeProvider _timeProvider;

        public BookingStatusUpdateService(
            IServiceProvider serviceProvider,
            ILogger<BookingStatusUpdateService> logger,
            TimeProvider timeProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timeProvider = timeProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingStatusUpdateService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateMissedBookingsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating missed bookings.");
                }

                // Run every 1 hour. Adjust as needed.
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("BookingStatusUpdateService is stopping.");
        }

        private async Task UpdateMissedBookingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var bookingRepo = unitOfWork.Repository<Booking, int>();

            var localNow = _timeProvider.GetLocalNow();
            // The local time provider is configured for Egypt Standard Time (Cairo)
            var today = DateOnly.FromDateTime(localNow.Date);

            // Any confirmed booking before today's date should be marked as Didn't Arrive
            var missedBookings = await bookingRepo.FindAsync(b => 
                b.Status == BookingStatus.Confirmed && 
                b.BookingDate < today);

            var count = missedBookings.Count();
            if (count > 0)
            {
                var utcNow = _timeProvider.GetUtcNow().DateTime;
                foreach (var booking in missedBookings)
                {
                    booking.Status = BookingStatus.DidNotArrive;
                    booking.UpdatedAt = utcNow;
                    bookingRepo.Update(booking);
                }

                await unitOfWork.CompleteAsync();
                _logger.LogInformation("Updated {Count} missed bookings to DidNotArrive.", count);
            }
        }
    }
}
