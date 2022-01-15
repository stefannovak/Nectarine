using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests
{
    public class AuthenticateSocialUserDTO
    {
        [Required]
        [MaxLength(2048)]
        public string Token { get; set; } = string.Empty;
    }
}