using System;
using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Generic
{
    public class UserDTO
    {
        public string? Id { get; set; }

        [EmailAddress]
        [Required]
        public string? Email { get; set; }
    }
}