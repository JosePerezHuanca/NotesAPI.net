using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Dto;
using NotesAPI.Models;
using NotesAPI.Repository;
using NotesAPI.Response;

namespace NotesAPI.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NoteService> _logger;

        public NoteService(INoteRepository noteRepository, IUserRepository userRepository, IMapper mapper, ILogger<NoteService> logger)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<List<NoteResponse>>> GetNotesAsync(int userId)
        {
            try
            {
                var notes = await _noteRepository.GetNotesByUserIdAsync(userId);
                var notesResponse = _mapper.Map<List<NoteResponse>>(notes);
                return new ApiResponse<List<NoteResponse>>(success: true, status: 200, data: notesResponse);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse<List<NoteResponse>>(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse<List<NoteResponse>>(success: false, status: 500, message: "Internal server error");
            }
        }

        public async Task<ApiResponse<NoteResponse>> GetNoteAsync(int noteId, int userId)
        {
            try
            {
                var note = await _noteRepository.GetNoteByIdAndUserIdAsync(noteId, userId);
                if (note == null)
                {
                    return new ApiResponse<NoteResponse>(success: false, status: 404, message: "Not found");
                }
                var noteResponse = _mapper.Map<NoteResponse>(note);
                return new ApiResponse<NoteResponse>(success: true, status: 200, data: noteResponse);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse<NoteResponse>(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse<NoteResponse>(success: false, status: 500, message: "Internal server error");
            }
        }

        public async Task<ApiResponse<NoteResponse>> PostNoteAsync(int userId, NoteDto noteDto)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NoteResponse>(success: false, status: 401, message: "Unauthorized");
                }
                var note = _mapper.Map<Note>(noteDto);
                note.UserId = userId;
                note.CreatedAt = DateTime.UtcNow;
                var createdNote = await _noteRepository.AddNoteAsync(note);
                var noteResponse = _mapper.Map<NoteResponse>(createdNote);
                return new ApiResponse<NoteResponse>(success: true, status: 201, data: noteResponse);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse<NoteResponse>(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse<NoteResponse>(success: false, status: 500, message: "Internal server error");
            }
        }

        public async Task<ApiResponse> PutNoteAsync(int userId, int noteId, NoteDto noteDto)
        {
            try
            {
                var note = await _noteRepository.GetNoteByIdAndUserIdAsync(noteId, userId);
                if (note == null)
                {
                    return new ApiResponse(success: false, status: 404, message: "Not found");
                }
                _mapper.Map(noteDto, note);
                note.UpdatedAt = DateTime.UtcNow;
                await _noteRepository.UpdateNoteAsync(note);
                return new ApiResponse(true, 204);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse(success: false, status: 500, message: "Internal server error");
            }
        }

        public async Task<ApiResponse> DeleteNoteAsync(int userId, int noteId)
        {
            try
            {
                var note = await _noteRepository.GetNoteByIdAndUserIdAsync(noteId, userId);
                if (note == null)
                {
                    return new ApiResponse(success: false, status: 404, message: "Not found");
                }
                await _noteRepository.DeleteNoteAsync(note);
                return new ApiResponse(true, 204);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                return new ApiResponse(success: false, status: 500, message: "Internal database error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                return new ApiResponse(success: false, status: 500, message: "Internal server error");
            }
        }
    }
}
