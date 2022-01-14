using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests;

public class UpdatePhoneNumberDTO
{
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}