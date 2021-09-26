using Stripe;

namespace nectarineAPI.DTOs.Requests
{
    public class UpdateUserDTO
    {
        public string? Email { get; set; }

        /// <summary>
        /// Uses the <see cref="Address"/> provided by Stripe.
        /// </summary>
        public AddressOptions? Address { get; set; }
    }
}