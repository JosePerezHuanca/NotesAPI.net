using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using NotesAPI.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NotesAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly NoteContext _context;
        private readonly ILogger<NotesController> _logger;
        public NotesController(NoteContext context, ILogger<NotesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notes = await _context.Notes
                    .Where(n => n.UserId == userId)
                    .Include(n => n.User)
                    .Select(n => new NoteResponse
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        Autor = n.User.Username,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                .ToListAsync();
                return Ok(notes);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var note = await _context.Notes
                    .Include(n => n.User)
                    .Where(n => n.Id == id && n.UserId == userId)
                    .Select(n => new NoteResponse
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        Autor = n.User.Username,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                .FirstOrDefaultAsync();
                if (note == null)
                {
                    return NotFound();
                }
                return Ok(note);
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

        [HttpPost]
        public async Task<IActionResult> PostNote(NoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Unauthorized();
                }
                var note = new Note
                {
                    Title = noteDto.Title,
                    Content = noteDto.Content,
                    CreatedAt = DateTime.UtcNow,
                    UserId=userId
                };
                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
                var noteResponse = new NoteResponse
                {
                    Id = note.Id,
                    Title = note.Title,
                    Content = note.Content,
                    Autor = user.Username,
                    CreatedAt = note.CreatedAt,
                    UpdatedAt = note.UpdatedAt
                };
                return CreatedAtAction(nameof(GetNote), new { id = note.Id }, noteResponse);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, NoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                if (note == null)
                {
                    return NotFound();
                }
                note.Title = noteDto.Title;
                note.Content = noteDto.Content;
                note.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return NoContent();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                if (note == null)
                {
                    return NotFound();
                }
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                return NoContent();
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
