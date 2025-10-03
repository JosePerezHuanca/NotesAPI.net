using MailKit.Net.Smtp;
using MimeKit;
using NotesAPI.Email;
using System.Threading.Channels;

namespace NotesAPI.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly Channel<EmailRequest> _channel;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(Channel<EmailRequest> channel, ILogger<EmailBackgroundService> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var client = new SmtpClient();
            try
            {
                string smtpServer = Environment.GetEnvironmentVariable("SMTP_HOST");
                int smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
                string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
                string smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls, stoppingToken);
                await client.AuthenticateAsync(smtpUser, smtpPass, stoppingToken);
                _logger.LogInformation("SMTP client connected and authenticated.");
                while (!stoppingToken.IsCancellationRequested)
                {
                    while (_channel.Reader.TryRead(out var emailRequest))
                    {
                        try
                        {
                            // Envío real del correo
                            await SendEmailAsync(emailRequest, client);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending email to {To}", emailRequest.To);
                        }
                    }
                    await _channel.Reader.WaitToReadAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Esperado cuando se cancela el servicio
                _logger.LogInformation("Email background service stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP client setup or connection failed.");
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, stoppingToken);
                    _logger.LogInformation("SMTP client disconnected.");
                }
            }
        }

        private async Task SendEmailAsync(EmailRequest request, SmtpClient client)
        {
            string fromAddress = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
            string fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Notes App";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(request.To));
            message.Subject = request.Subject;
            message.Body = new TextPart("plain") { Text = request.Body };

            await client.SendAsync(message);
            _logger.LogInformation("Email sent successfully to {Recipient}", request.To);
        }
    }
}
