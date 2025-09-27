using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotesAPI.Controllers;
using NotesAPI.Dto;
using NotesAPI.Models;
using NotesAPI.Repository;
using NotesAPI.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotesAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IUserRepository userRepository, IMapper mapper, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                string normalizedUsername = request.Username.Trim().ToLower();
                string normalizedEmail = request.Email.Trim().ToLower();
                var conflicts = new Dictionary<string, List<string>>();
                if (await _userRepository.IsUsernameInUseAsync(normalizedUsername))
                {
                    conflicts.Add("username", new List<string> { "The username is already in use." });
                }
                if (await _userRepository.IsEmailInUseAsync(normalizedEmail))
                {
                    conflicts.Add("email", new List<string> { "The email is already in use." });
                }
                if (conflicts.Any())
                {
                    return new ApiResponse<string>(success: false, status: 409, errors: conflicts);
                }
                var passHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = _mapper.Map<User>(request);
                user.Username = normalizedUsername;
                user.Email = normalizedEmail;
                user.Password = passHash;
                await _userRepository.AddUserAsync(user);
                return new ApiResponse<string>(success: true, status: 200, message: "User registered successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse<string>(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse<string>(success: false, status: 500, message: "Internal server error");
            }
        }

        public async Task<ApiResponse<string>> LoginUserAsync(LoginRequest request)
        {
            try
            {
                string identifier = request.LoginIdentifier.Trim().ToLower();
                var userQuery = await _userRepository.GetUserByUsernameOrEmailAsync(identifier);
                if (userQuery == null || !BCrypt.Net.BCrypt.Verify(request.Password, userQuery.Password))
                {
                    return new ApiResponse<string>(success: false, status: 401, message: "Unauthorized");
                }
                var tokenResponse = generateToken(userQuery.UserId);
                return new ApiResponse<string>(success: true, status: 200, token: tokenResponse);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse<string>(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse<string>(success: false, status: 500, message: "Internal server error");
            }
        }

        private string generateToken(int userId)
        {
            var tokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET");
            var tokenIssuer = Environment.GetEnvironmentVariable("TOKEN_ISSUER");
            var tokenAudience = Environment.GetEnvironmentVariable("TOKEN_AUDIENCE");
            if(string.IsNullOrEmpty(tokenSecret) || string.IsNullOrEmpty(tokenIssuer) || string.IsNullOrEmpty(tokenAudience))
            {
                throw new InvalidOperationException("Token configuration is missing");
            }
            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                };
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
            var creds = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: tokenIssuer,
                audience: tokenAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
