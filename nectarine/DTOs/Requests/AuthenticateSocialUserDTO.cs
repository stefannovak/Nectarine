using System;
using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests
{
    public class AuthenticateSocialUserDTO
    {
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;
    }
}