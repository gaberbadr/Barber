using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendWelcomeEmailAsync(string recipientEmail, string userName, string activationLink);
        Task SendPasswordResetEmailAsync(string recipientEmail, string userName, string resetLink);
        Task SendConfirmationEmailAsync(string recipientEmail, string userName, string message);
        Task SendNotificationEmailAsync(string recipientEmail, string userName, string title, string content);
        Task SendOtpEmailAsync(string recipientEmail, string userName, string otp, int? expirationMinutes = 3);
        Task SendBookingInfoAsync(string barberEmail, string barberName, string customerName, string customerPhone, DateOnly bookingDate, TimeOnly startTime);
        Task SendBookingCancellationAsync(string barberEmail, string barberName, string customerName, string customerPhone, DateOnly bookingDate, TimeOnly startTime);
    }
}
