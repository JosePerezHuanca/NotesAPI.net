using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesAPI.Dto;
using NotesAPI.Response;
using NotesAPI.Services;
using System.Security.Claims;

namespace NotesAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NotesController> _logger;
        public NotesController(INoteService noteService, ILogger<NotesController> logger)
        {
            _noteService = noteService;
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

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _noteService.GetNotesAsync(userId);
            return result.Status switch
            {
                200 => Ok(result),
                _ => StatusCode(500, result)
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _noteService.GetNoteAsync(id, userId);
            return result.Status switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                _ => StatusCode(500, result)
            };
        }

        [HttpPost]
        public async Task<IActionResult> PostNote(NoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
            }
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _noteService.PostNoteAsync(userId, noteDto);
            return result.Status switch
            {
                201 => CreatedAtAction(nameof(GetNote), new { id = result.Data?.Id },result),
                401 => Unauthorized(result),
                _ => StatusCode(500, result)
            };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, NoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
            }
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _noteService.PutNoteAsync(userId, id, noteDto);
            return result.Status switch
            {
                204 => NoContent(),
                404 => NotFound(result),
                _ => StatusCode(500, result)
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _noteService.DeleteNoteAsync(userId, id);
            return result.Status switch
            {
            204 => NoContent(),
            404 => NotFound(result),
            _ => StatusCode(500, result)
            };
        }
    }
}
