using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Models
{
    public class Note
    {
        public int Id { get; set; }
        ['Required']
        public string Title { get; set; }
        ['Required']
        public string Content { get; set; }
        ['Required']
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
