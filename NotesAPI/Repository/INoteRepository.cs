using NotesAPI.Models;

namespace NotesAPI.Repository
{
    public interface INoteRepository
    {
        Task<List<Note>> GetNotesByUserIdAsync(int userId);
        Task<Note> GetNoteByIdAndUserIdAsync(int noteId, int userId);
        Task<Note> AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(Note note);
    }
}
