using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Models
{
    public class User
    {
        public int UserId {  get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        public string Password { get; set; }
    }
}
