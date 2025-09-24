using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using NotesAPI.Models;

namespace NotesAPI.Repository
{
    public class NoteRepository : INoteRepository
    {
        private readonly NoteContext _context;
        public NoteRepository(NoteContext context)
        {
            _context = context;
        }
        public async Task<List<Note>> GetNotesByUserIdAsync(int userId)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId)
                .Include(n => n.User)
                .ToListAsync();
        }

        public async Task<Note> GetNoteByIdAndUserIdAsync(int noteId, int userId)
        {
            return await _context.Notes
                .Include(n => n.User)
                .SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
        }

        public async Task<Note> AddNoteAsync(Note note)
        {
            await _context.Notes.AddAsync(note);
            await _context.SaveChangesAsync();
            return await _context.Notes.Include(n => n.User).FirstOrDefaultAsync(n => n.Id == note.Id);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(Note note)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
    }
}
