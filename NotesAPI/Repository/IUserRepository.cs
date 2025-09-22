using NotesAPI.Models;

namespace NotesAPI.Repository
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameOrEmailAsync(string identifier);
        Task<bool> IsUsernameInUseAsync(string username);
        Task<bool> IsEmailInUseAsync(string email);
        Task AddUserAsync(User user);
    }
}
