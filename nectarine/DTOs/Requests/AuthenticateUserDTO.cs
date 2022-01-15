using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests
{
    public class AuthenticateUserDTO
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