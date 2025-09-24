using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Repository
{
    public class UserRepository: IUserRepository
    {
        private readonly NoteContext _context;

        public UserRepository(NoteContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameOrEmailAsync(string identifier)
        {
            bool isEmail = new EmailAddressAttribute().IsValid(identifier);
            return await _context.Users.SingleOrDefaultAsync(u =>
            isEmail
                ? u.Email.ToLower() == identifier
                : u.Username.ToLower() == identifier
            );
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> IsUsernameInUseAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailInUseAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
