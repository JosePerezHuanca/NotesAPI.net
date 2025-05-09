using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using NotesAPI.Dto;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

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

        [HttpPost("register")]
        public async Task<IActionResult> PostRegister(RegisterRequest request)
        {
            string normalizedUsername = request.Username.Trim().ToLower();
            string normalizedEmail = request.Email.Trim().ToLower();
            if(await _context.Users.AnyAsync(u => u.Username == normalizedUsername))
            {
                ModelState.AddModelError("Username", "The username is already in use.");
            }
            if(await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
            {
                ModelState.AddModelError("Email", "The email is already in use.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var passHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Username = normalizedUsername,
                Email = normalizedEmail,
                Password = passHash,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginRequest request)
        {
            string identifier = request.LoginIdentifier.Trim().ToLower();
            bool isEmail= new EmailAddressAttribute().IsValid(identifier);
            var userQuery = await _context.Users.SingleOrDefaultAsync(u =>
            isEmail
                ? u.Email.ToLower() == identifier
                : u.Username.ToLower() == identifier
            );
            if (userQuery == null || !BCrypt.Net.BCrypt.Verify(request.Password, userQuery.Password))
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userQuery.UserId.ToString())
            };
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")));
            var creds = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("TOKEN_ISSUER"),
                audience: Environment.GetEnvironmentVariable("TOKEN_AUDIENCE"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            var tokenResponse = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new {token= tokenResponse });
        }
    }
}
