using System.ComponentModel.DataAnnotations;
using Stripe;

namespace NectarineAPI.DTOs.Requests
{
    public class UpdateAddressDTO
    {
        /// <summary>
        /// Uses the <see cref="Address"/> provided by Stripe.
        /// </summary>
        [Required]
        public AddressOptions Address { get; set; } = null!;

        public bool IsPrimaryAddress { get; set; }
    }
}