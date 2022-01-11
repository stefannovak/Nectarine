using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests
{
    public class AuthenticateSocialUserDTO
    {
        [Required]
        [MaxLength(2048)]
        public string Token { get; set; } = string.Empty;
    }
}