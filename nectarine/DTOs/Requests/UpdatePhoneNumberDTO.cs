using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests;

public class UpdatePhoneNumberDTO
{
    public UpdatePhoneNumberDTO(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    // Todo: - Maybe some validation?
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; }
}