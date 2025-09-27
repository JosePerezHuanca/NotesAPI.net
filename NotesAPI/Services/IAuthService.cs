using NotesAPI.Dto;
using NotesAPI.Response;

namespace NotesAPI.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest request);
        Task<ApiResponse<string>> LoginUserAsync(LoginRequest request);
    }
}
