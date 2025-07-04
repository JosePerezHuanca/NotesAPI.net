using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using NotesAPI.Dto;
using NotesAPI.Response;
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

        private Dictionary<string, List<string>> GetModelErrors()
        {
            return ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostRegister(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
            }
            try
            {
                string normalizedUsername = request.Username.Trim().ToLower();
                string normalizedEmail = request.Email.Trim().ToLower();
                var conflicts = new Dictionary<string, List<string>>();
                if (await _context.Users.AnyAsync(u => u.Username == normalizedUsername))
                {
                    conflicts.Add("username", new List<string> { "The username is already in use."});
                }
                if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
                {
                    conflicts.Add("email", new List<string> { "The email is already in use." });
                }
                if (conflicts.Any())
                {
                    return Conflict(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 409, errors: conflicts));
                }
                var passHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = _mapper.Map<User>(request);
                user.Username = normalizedUsername;
                user.Email = normalizedEmail;
                user.Password = passHash;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse<string>(success: true, status: 200, message: "User registered successfully"));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return StatusCode(500, new ApiResponse<string>(success: false, status: 500, message: "Internal database error"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Server error");
                return StatusCode(500, new ApiResponse<string>(success: false, status: 500, message: "Internal server error"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
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
                    return Unauthorized(new ApiResponse<string>(success: false, status: 401, message: "Unauthorized"));
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
                return Ok(new ApiResponse<string>(success: true, status: 200, token: tokenResponse));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return StatusCode(500, new ApiResponse<string>(success: false, status: 500, message: "Internal database error"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return StatusCode(500, new ApiResponse<string>(success: false, status: 500, message: "Internal server error"));
            }
        }
    }
}
