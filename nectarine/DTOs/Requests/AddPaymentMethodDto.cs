using System;
using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests
{
    public class AddPaymentMethodDto
    {
        [MaxLength(16)]
        public string CardNumber { get; set; } = string.Empty;
        
        [MaxLength(2)]
        public int ExpiryMonth { get; set; }
        
        [MaxLength(4)]
        public int ExpiryYear { get; set; }
        
        [MaxLength(3)]
        public string CVC { get; set; } = string.Empty;
    }
}