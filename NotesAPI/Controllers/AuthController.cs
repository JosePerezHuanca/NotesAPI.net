using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

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
                return Unauthorized();
            }
            bool isValid = BCrypt.Net.BCrypt.Verify(user.Password, userQuery.Password);
            if (isValid)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userQuery.UserId.ToString())
                };
                var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")));
                var creds = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: Environment.GetEnvironmentVariable("TOKEN_ISSUER"),
                    audience: Environment.GetEnvironmentVariable("TOKEN_AUDIENCE"),
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );
                var tokenResponse = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new {token= tokenResponse });
            }
            return Unauthorized();
        }
    }
}
