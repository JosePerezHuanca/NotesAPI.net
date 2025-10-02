namespace NotesAPI.Services
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
