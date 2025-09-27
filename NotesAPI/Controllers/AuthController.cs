using Microsoft.AspNetCore.Mvc;
using NotesAPI.Dto;
using NotesAPI.Response;
using NotesAPI.Services;

namespace NotesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
            var result = await _authService.RegisterUserAsync(request);
            return result.Status switch
            {
                200 => Ok(result),
                409 => Conflict(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, new ApiResponse<string>(success: false, status: 500, message: "Unexpected error"))
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
            }
            var result = await _authService.LoginUserAsync(request);
            return result.Status switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, new ApiResponse<string>(success: false, status: 500, message: "Unexpected error"))
            };
        }
    }
}
