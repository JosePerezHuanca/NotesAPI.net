using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using BCrypt.Net;

namespace NotesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly NoteContext _context;
        public AuthController(NoteContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users=await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostRegister(User user)
        {
            var userQuery = await _context.Users.FirstOrDefaultAsync(u=>u.Username == user.Username);
            if (userQuery != null)
            {
                return Conflict();
            }
            var passHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = passHash;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(User user)
        {
            var userQuery = await _context.Users.SingleOrDefaultAsync(u => u.Username == user.Username);
            if (userQuery == null)
            {
                return BadRequest();
            }
            bool isValid = BCrypt.Net.BCrypt.Verify(user.Password, userQuery.Password);
            if (isValid)
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}
