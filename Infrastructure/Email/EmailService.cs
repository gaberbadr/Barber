using Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
    public class EmailService : IEmailSender
    {
        private readonly SmtpEmailSender _smtpEmailSender;
        private readonly EmailOptions _emailOptions;

        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
            _smtpEmailSender = new SmtpEmailSender(emailOptions);
        }

        public async Task SendWelcomeEmailAsync(string recipientEmail, string userName, string activationLink)
        {
            var htmlBody = Templates.WelcomeEmailTemplate(userName, activationLink);
            await _smtpEmailSender.SendEmailAsync(recipientEmail, "Welcome to Our Platform", htmlBody);
        }

        public async Task SendPasswordResetEmailAsync(string recipientEmail, string userName, string resetLink)
        {
            var htmlBody = Templates.PasswordResetTemplate(userName, resetLink);
            await _smtpEmailSender.SendEmailAsync(recipientEmail, "Password Reset Request", htmlBody);
        }

        public async Task SendConfirmationEmailAsync(string recipientEmail, string userName, string message)
        {
            var htmlBody = Templates.ConfirmationEmailTemplate(userName, message);
            await _smtpEmailSender.SendEmailAsync(recipientEmail, "Confirmation", htmlBody);
        }

        public async Task SendNotificationEmailAsync(string recipientEmail, string userName, string title, string content)
        {
            var htmlBody = Templates.NotificationEmailTemplate(userName, title, content);
            await _smtpEmailSender.SendEmailAsync(recipientEmail, title, htmlBody);
        }

        public async Task SendOtpEmailAsync(string recipientEmail, string userName, string otp, int? expirationMinutes = null)
        {
            var expiration = expirationMinutes ?? _emailOptions.ExpirationMinutes;
            var htmlBody = Templates.OtpEmailTemplate(userName, otp, expiration);
            await _smtpEmailSender.SendEmailAsync(recipientEmail, "Your One-Time Password (OTP)", htmlBody);
        }

        public async Task SendBookingInfoAsync(string barberEmail, string barberName, string customerName, string customerPhone, DateOnly bookingDate, TimeOnly startTime)
        {
            var htmlBody = Templates.BookingInfoTemplate(barberName, customerName, customerPhone, bookingDate, startTime);
            await _smtpEmailSender.SendEmailAsync(barberEmail, "New Booking Notification", htmlBody);
        }

        public async Task SendBookingCancellationAsync(string barberEmail, string barberName, string customerName, string customerPhone, DateOnly bookingDate, TimeOnly startTime)
        {
            var htmlBody = Templates.BookingCancellationTemplate(barberName, customerName, customerPhone, bookingDate, startTime);
            await _smtpEmailSender.SendEmailAsync(barberEmail, "Booking Cancellation Notice", htmlBody);
        }
    }
}
