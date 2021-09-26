using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests
{
    public class AddPaymentMethodDTO
    {
        [StringLength(16, MinimumLength = 16)]
        public string CardNumber { get; set; } = string.Empty;
        
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }
        
        [Range(2021, 2030)]
        public int ExpiryYear { get; set; }
        
        [StringLength(4, MinimumLength = 3)]
        public string CVC { get; set; } = string.Empty;
    }
}