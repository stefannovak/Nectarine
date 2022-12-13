using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests;

public class UpdatePhoneNumberDTO
{
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}