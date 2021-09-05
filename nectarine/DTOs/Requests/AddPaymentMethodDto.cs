using System;
using System.ComponentModel.DataAnnotations;

namespace nectarineAPI.DTOs.Requests
{
    public class AddPaymentMethodDto
    {
        [Range(16,16)]
        public string CardNumber { get; set; } = string.Empty;
        
        [Range(2,2)]
        public int ExpiryMonth { get; set; }
        
        [Range(2,2)]
        public int ExpiryYear { get; set; }
        
        [Range(3,4)]
        public string CVC { get; set; } = string.Empty;
    }
}