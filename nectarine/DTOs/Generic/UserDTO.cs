using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Generic
{
    public class UserDTO
    {
        public string? Id { get; set; }

        [EmailAddress]
        [Required]
        public string? Email { get; set; }
    }
}