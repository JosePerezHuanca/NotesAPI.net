using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NotesAPI.Models
{
    [Index(nameof(Username), IsUnique =true)]
    [Index(nameof(Email), IsUnique =true)]
    public class User
    {
        public int UserId {  get; set; }
        [Required]
        [StringLength(100)]
        public string Username { get; set; }
        [Required]
        [StringLength(250)]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        [StringLength(250)]
        public string Password { get; set; }
        public ICollection<Note> Notes { get; set; }
    }
}
