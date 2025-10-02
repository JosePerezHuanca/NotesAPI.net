using MailKit.Net.Smtp;
using MimeKit;

namespace NotesAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            string smtpServer = Environment.GetEnvironmentVariable("SMTP_HOST");
            int smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            string smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
            string fromAddress = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
            string fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Notes App";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully to {Recipient}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                throw;
            }
        }
    }
}
