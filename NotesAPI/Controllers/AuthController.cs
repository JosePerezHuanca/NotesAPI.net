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
using AutoMapper;

namespace NotesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly NoteContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        public AuthController(NoteContext context, IMapper mapper, ILogger<AuthController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostRegister(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string normalizedUsername = request.Username.Trim().ToLower();
                string normalizedEmail = request.Email.Trim().ToLower();
                var conflicts = new Dictionary<string, string>();
                if (await _context.Users.AnyAsync(u => u.Username == normalizedUsername))
                {
                    conflicts.Add("username", "The username is already in use.");
                }
                if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
                {
                    conflicts.Add("email", "The email is already in use.");
                }
                if (conflicts.Any())
                {
                    return Conflict(conflicts);
                }
                var passHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = _mapper.Map<User>(request);
                user.Username = normalizedUsername;
                user.Email = normalizedEmail;
                user.Password = passHash;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return StatusCode(500, new{ message ="Internal database error."});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Server error");
                return StatusCode(500, new {message= "Internal server error."});
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string identifier = request.LoginIdentifier.Trim().ToLower();
                bool isEmail = new EmailAddressAttribute().IsValid(identifier);
                var userQuery = await _context.Users.SingleOrDefaultAsync(u =>
                isEmail
                    ? u.Email.ToLower() == identifier
                    : u.Username.ToLower() == identifier
                );
                if (userQuery == null || !BCrypt.Net.BCrypt.Verify(request.Password, userQuery.Password))
                {
                    return Unauthorized();
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
                return Ok(new { token = tokenResponse });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return StatusCode(500, new { message = "Internal database error." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return StatusCode(500, new { message = "Internal server error." });
            }
            
        }
    }
}
