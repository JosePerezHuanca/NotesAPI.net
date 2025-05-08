using System.ComponentModel.DataAnnotations;

namespace NotesAPI.Dto
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        public string Username {  get; set; }
        [Required]
        [StringLength(250)]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        [StringLength(250)]
        public string Password {  get; set; }
    }
}
