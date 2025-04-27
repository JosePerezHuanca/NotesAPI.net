using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Dto
{
    public class NoteDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
