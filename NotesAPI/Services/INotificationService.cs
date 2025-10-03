namespace NotesAPI.Services
{
    public interface INotificationService
    {
        Task AddEmailToQueueAsync(string to, string subject, string body);
    }
}
