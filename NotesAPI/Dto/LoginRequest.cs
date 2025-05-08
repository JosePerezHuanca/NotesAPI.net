using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Dto
{
    public class LoginRequest
    {
        [Required]
        [StringLength(250)]
        public string LoginIdentifier { get; set; }
        [Required]
        [StringLength(250)]
        public string Password { get; set; }
    }
}
