using MailKit.Net.Smtp;
using MimeKit;
using NotesAPI.Email;
using System.Threading.Channels;

namespace NotesAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly Channel<EmailRequest> _channel;
        public NotificationService(ILogger<NotificationService> logger, Channel<EmailRequest> channel)
        {
            _logger = logger;
            _channel = channel;
        }

        public async Task AddEmailToQueueAsync(string to, string subject, string body)
        {
            var request = new EmailRequest { To = to, Subject = subject, Body = body };
            await _channel.Writer.WriteAsync(request);
            _logger.LogInformation("Queued email to {Recipient}", to);
        }
    }
}
