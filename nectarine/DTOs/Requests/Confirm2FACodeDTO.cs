using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests;

public class Confirm2FACodeDTO
{
    [Required]
    public int Code { get; set; }
}