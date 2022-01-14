using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests;

public class Confirm2FACodeDTO
{
    [Required]
    public int Code { get; set; }
}