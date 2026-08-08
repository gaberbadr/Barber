using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email
{
    public class SmtpEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public SmtpEmailSender(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody)
        {
            using (var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port))
            {
                client.EnableSsl = _emailOptions.EnableSsl;
                client.Credentials = new NetworkCredential(_emailOptions.SenderEmail, _emailOptions.SenderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailOptions.SenderEmail, _emailOptions.SenderDisplayName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}