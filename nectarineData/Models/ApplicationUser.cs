using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NectarineData.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string PaymentProviderCustomerId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public int? VerificationCode { get; set; }

        [Required]
        public virtual IList<ExternalAuthLink> SocialLinks { get; set; } = new List<ExternalAuthLink>();

        [MaxLength(100)]
        public Guid? CurrentShippingAddressId { get; set; }

        /// <summary>
        /// Overriden from <see cref="IdentityUser"/> as Email is the basis for creating users. It must not be null.
        /// </summary>
        [Required]
        [EmailAddress]
        public override string Email { get; set; } = string.Empty;

        public virtual IList<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    }
}