using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests
{
    public class AuthenticateSocialUserDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}