using NotesAPI.Dto;
using NotesAPI.Response;

namespace NotesAPI.Services
{
    public interface INoteService
    {
        Task<ApiResponse<List<NoteResponse>>> GetNotesAsync(int userId);
        Task<ApiResponse<NoteResponse>> GetNoteAsync(int noteId, int userId);
        Task<ApiResponse<NoteResponse>> PostNoteAsync(int userId, NoteDto note);
        Task<ApiResponse> PutNoteAsync(int userId, int noteId, NoteDto note);
        Task<ApiResponse> DeleteNoteAsync(int userId, int noteId);
    }
}
