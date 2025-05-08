using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Dto
{
    public class LoginRequest
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }
        [Required]
        [StringLength(250)]
        public string Password { get; set; }
    }
}
