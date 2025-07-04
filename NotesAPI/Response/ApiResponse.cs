namespace NotesAPI.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public T? Data { get; set; }
        public Dictionary<string, List<string>>? Errors { get; set; }

        public ApiResponse(bool success, int status, string? message = null, string? token = null, T? data = default, Dictionary<string, List<string>>? errors = null)
        {
            Success = success;
            Status = status;
            Message = message;
            Token = token;
            Data = data;
            Errors = errors;
        }
    }
}
