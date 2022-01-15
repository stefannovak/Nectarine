using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests
{
    public class CreateUserDTO
    {
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}