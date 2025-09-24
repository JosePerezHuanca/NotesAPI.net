using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Repository;
using NotesAPI.Models;
using NotesAPI.Dto;
using NotesAPI.Response;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;

namespace NotesAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NotesController> _logger;
        public NotesController(INoteRepository noteRepository, IUserRepository userRepository,IMapper mapper, ILogger<NotesController> logger)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
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

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notes = await _noteRepository.GetNotesByUserIdAsync(userId);
                var notesResponse = _mapper.Map<List<NoteResponse>>(notes);
                return Ok(new ApiResponse<List<NoteResponse>>(success: true, status: 200, data: notesResponse));
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var note = await _noteRepository.GetNoteByIdAndUserIdAsync(id, userId);
                if (note == null)
                {
                    return NotFound(new ApiResponse<string>(success: false, status: 404, message: "Not found"));
                }
                var noteResponse = _mapper.Map<NoteResponse>(note);
                return Ok(new ApiResponse<NoteResponse>(success: true, status: 200, data: noteResponse));
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

        [HttpPost]
        public async Task<IActionResult> PostNote(NoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
            }
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse<string>(success: false, status: 401, message: "Unauthorized"));
                }
                var note = _mapper.Map<Note>(noteDto);
                note.UserId = userId;
                note.CreatedAt = DateTime.UtcNow;
                var createdNote = await _noteRepository.AddNoteAsync(note);
                var noteResponse = _mapper.Map<NoteResponse>(createdNote);
                return CreatedAtAction(nameof(GetNote), new { id = note.Id }, new ApiResponse<NoteResponse>(success: true, status: 201, data: noteResponse));
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, NoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelErrors();
                return BadRequest(new ApiResponse<Dictionary<string, List<string>>>(success: false, status: 400, errors: errors));
            }
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var note = await _noteRepository.GetNoteByIdAndUserIdAsync(id, userId);
                if (note == null)
                {
                    return NotFound(new ApiResponse<string>(success: false, status: 404, message: "Not found"));
                }
                _mapper.Map(noteDto, note);
                note.UpdatedAt = DateTime.UtcNow;
                await _noteRepository.UpdateNoteAsync(note);
                return NoContent();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var note = await _noteRepository.GetNoteByIdAndUserIdAsync(id, userId);
                if (note == null)
                {
                    return NotFound(new ApiResponse<string>(success: false, status: 404, message: "Not found"));
                }
                await _noteRepository.DeleteNoteAsync(note);
                return NoContent();
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
