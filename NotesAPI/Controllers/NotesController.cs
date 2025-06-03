using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;
using NotesAPI.Dto;

namespace NotesAPI.Controllers
{
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
                var notes = await _context.Notes.ToListAsync();
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
                var note = await _context.Notes.FindAsync(id);
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
                var note = new Note
                {
                    Title = noteDto.Title,
                    Content = noteDto.Content,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
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
                var noteQuery = await _context.Notes.FindAsync(id);
                if (noteQuery == null)
                {
                    return NotFound();
                }
                noteQuery.Title = noteDto.Title;
                noteQuery.Content = noteDto.Content;
                noteQuery.UpdatedAt = DateTime.UtcNow;
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
                var note = await _context.Notes.FindAsync(id);
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
